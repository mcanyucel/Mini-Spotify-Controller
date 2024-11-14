using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using MiniSpotifyController.service;
using MiniSpotifyController.service.implementation;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.OAuth;

public sealed partial class OAuthAuthenticator(
    OAuthConfig config,
    ITokenStorage tokenStorage,
    IPreferenceService preferenceService,
    IWindowService windowService,
    IHttpClientFactory httpClientFactory) : IDisposable
{
    public string? AccessToken { get; private set; }
    
    
    private readonly SemaphoreSlim _authorizationSemaphore = new(1, 1);
    private bool _isApiInitialized;

    private void EnsureApiInitialized()
    {
        if (_isApiInitialized) return;
        var clientId = EnsureClientId();
        config.ClientId = clientId;
        _isApiInitialized = true;
    }

    private async Task EnsureAuthorized()
    {
        if (!await IsAuthorized())
        {
            await Authorize();
        }
    }

    public async Task<bool> IsAuthorized()
    {
        ObjectDisposedException.ThrowIf(_disposedValue, this);
        EnsureApiInitialized();
        
        await _authorizationSemaphore.WaitAsync();
        var hasAccessToken = !string.IsNullOrEmpty(AccessToken);

        if (!hasAccessToken)
        {
            var refreshToken = tokenStorage.RetrieveRefreshToken();
            // try to refresh the access token
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    var refreshTokenResponse = await RefreshAccessToken(refreshToken);
                    if (refreshTokenResponse is { AccessToken: not null })
                    {
                        AccessToken = refreshTokenResponse.AccessToken;
                        hasAccessToken = true;
                    }
                }
                catch
                {
                    // if we cannot refresh the token, we need to reauthorize - clear the expired refresh token
                    tokenStorage.ClearRefreshToken();
                }
            }

        }
        _authorizationSemaphore.Release();
        return hasAccessToken;
    }

    public async Task<bool> Authorize()
    {
        ObjectDisposedException.ThrowIf(_disposedValue, this);
        EnsureApiInitialized();
        
        await _authorizationSemaphore.WaitAsync();
        try
        {
            var storedRefreshToken = tokenStorage.RetrieveRefreshToken();
            if (!string.IsNullOrEmpty(storedRefreshToken))
            {
                var refreshTokenResponse = await RefreshAccessToken(storedRefreshToken);
                if (refreshTokenResponse is { AccessToken: not null })
                {
                    AccessToken = refreshTokenResponse.AccessToken;
                }
                else
                {
                    await PerformFullAuthorization();
                }
            }
            else
            {
                await PerformFullAuthorization();
            }
        }
        finally
        {
            _authorizationSemaphore.Release();
        }
        
        return !string.IsNullOrEmpty(AccessToken);
    }

    private async Task PerformFullAuthorization()
    {
        var (authorizationCode, codeVerifier) = await GetAuthorizationCode();
        var authorizationResponse = await ExchangeAuthorizationCodeForTokens(authorizationCode, codeVerifier);
        if (authorizationResponse is { AccessToken: not null, RefreshToken: not null })
        {
            AccessToken = authorizationResponse.AccessToken;
            tokenStorage.StoreRefreshToken(authorizationResponse.RefreshToken);
        }
        else
        {
            throw new InvalidOperationException("Authorization failed");
        }
    }

    private async Task<AuthorizationResponse?> ExchangeAuthorizationCodeForTokens(string authorizationCode, string codeVerifier)
    {
        var httpClient = httpClientFactory.CreateClient(App.SpotifyAuthClientName);
        var authorizationRequest = CreateAuthorizationTokenRequest(authorizationCode, codeVerifier);
        var response = await httpClient.SendAsync(authorizationRequest);
        var responseJson = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        
        return JsonSerializer.Deserialize<AuthorizationResponse>(responseJson);
    }
    
    private HttpRequestMessage CreateAuthorizationTokenRequest(string authorizationCode, string codeVerifier)
    {
        var tokenRequestData = new Dictionary<string, string?>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = authorizationCode,
            ["redirect_uri"] = config.RedirectUrl,
            ["client_id"] = config.ClientId,
            ["code_verifier"] = codeVerifier
        };
        // The request must include the authorization and content-type headers in a specific format
        var authorizationRequestMessage = new HttpRequestMessage(HttpMethod.Post, config.TokenUrl)
        {
            Content = new FormUrlEncodedContent(tokenRequestData)
        };
        var contentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
        authorizationRequestMessage.Content.Headers.ContentType = contentType;
        return authorizationRequestMessage;
    }

    private async Task<(string authorizationCode, string codeVerifier)> GetAuthorizationCode()
    {
        var (codeVerifier, codeChallenge) = OAuthHelper.GenerateCodeChallengeAndVerifier();
        var authorizationRequestUrl = CreateAuthorizationRequestUrl(codeChallenge);

        using var httpListener = new HttpListener();
        httpListener.Prefixes.Add(config.RedirectUrl + "/");
        httpListener.Start();
        
        var authCodeTask = ListenForAuthorizationCode(httpListener);
        OAuthHelper.OpenBrowser(authorizationRequestUrl);
        
        var authorizationCode = await authCodeTask;
        return (authorizationCode, codeVerifier);
    }

    private string CreateAuthorizationRequestUrl(string codeChallenge)
    {
        var builder = new UriBuilder(config.AuthBaseUrl);
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["client_id"] = config.ClientId;
        query["response_type"] = "code";
        query["redirect_uri"] = config.RedirectUrl;
        query["state"] = config.State;
        query["scope"] = config.Scopes;
        query["code_challenge"] = codeChallenge;
        query["code_challenge_method"] = "S256";
        builder.Query = query.ToString();
        return builder.ToString();
    }

    private async Task<string> ListenForAuthorizationCode(HttpListener httpListener)
    {
        try
        {
            var context = await httpListener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            if (request.QueryString["state"] != config.State)
                throw new InvalidOperationException("State mismatch");


            var authorizationCode = request.QueryString["code"];
            if (string.IsNullOrEmpty(authorizationCode))
                throw new InvalidOperationException("Authorization code not found");

            OAuthHelper.SendResponseToBrowser(response);
            return authorizationCode;
        }
        finally
        {
            httpListener.Stop();
        }
    }

    private async Task<RefreshTokenResponse?> RefreshAccessToken(string refreshToken)
    {
        ObjectDisposedException.ThrowIf(_disposedValue, this);

        var tokenRequestData = new Dictionary<string, string?>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = config.ClientId,
        };
        var responseJson = await SendTokenRequest(tokenRequestData);
        var refreshTokenResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(responseJson);
        // if the access token is not null and there is a new refresh token, store it
        if (refreshTokenResponse is { AccessToken: not null, RefreshToken: not null })
        {
            tokenStorage.StoreRefreshToken(refreshTokenResponse.RefreshToken);
        }
        return refreshTokenResponse;
    }

    private async Task<string> SendTokenRequest(Dictionary<string, string?> tokenRequest)
    {
        var httpClient = httpClientFactory.CreateClient(App.SpotifyAuthClientName);
        var response = await httpClient.PostAsync(config.TokenUrl, new FormUrlEncodedContent(tokenRequest));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Tries to get the client ID from the preference service. If it is not set, it will show the client ID window and try again for a maximum of 3 times.
    /// If the client ID is still not set after 3 attempts, an exception is thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the client id cannot be set after 3 attempts</exception>
    private string EnsureClientId()
    {
            const int retryLimit = 3;
            var retryCount = 0;

            string? id = preferenceService.GetClientId();
            while (string.IsNullOrEmpty(id) && retryCount < retryLimit)
            {
                windowService.ShowWindow<ClientIdViewModel>(isModal: true);
                id = preferenceService.GetClientId();
                retryCount++;
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Client ID not set after multiple attempts");
            }
            return id;
    }
    
 
    private bool _disposedValue;
    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            // dispose managed state (managed objects)
            _authorizationSemaphore.Dispose();
            AccessToken = null;
        }

        // free unmanaged resources (unmanaged objects) and override finalizer
        // set large fields to null
        _disposedValue = true;
    }

    // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    //~OAuthAuthenticator()
    //{
    //    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //    Dispose(disposing: false);
    //}

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        // GC.SuppressFinalize(this); only if 'Dispose(bool disposing)' has code to free unmanaged resources
    }
}