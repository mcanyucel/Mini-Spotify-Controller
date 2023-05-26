using Mini_Spotify_Controller.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.service.implementation
{
    class SpotifyService : ISpotifyService, IDisposable
    {
        #region Properties
        AccessData? ISpotifyService.AccessData => m_AccessData;
        bool ISpotifyService.IsAuthorized => m_AccessData != null && m_AccessData.AccessToken != null;
        #endregion

        #region Lifecycle
        public SpotifyService(IPreferenceService preferenceService, IWindowService windowService)
        {
            m_PreferenceService = preferenceService;
            m_WindowService = windowService;
            clientId = m_PreferenceService.GetClientId();
        }
        public void Dispose()
        {
            httpClient.Dispose();
        }
        #endregion

        #region Authorization

        async Task ISpotifyService.Authorize()
        {
            if (clientId == null)
            {
                m_WindowService.ShowClientIdWindowDialog();
            }

            m_AccessData = await RefreshAccessToken();
            // If we still don't have an access token, we need to request one
            if (m_AccessData == null)
                ShowAccessTokenRequest();
        }

        async Task<AccessData?> RefreshAccessToken()
        {
            AccessData? result = null;
            string? refreshToken = m_PreferenceService.GetRefreshToken();
            if (refreshToken != null)
            {
                result = await RefreshAccessToken(refreshToken);
            }
            return result;
        }

        void ShowAccessTokenRequest()
        {
            m_WindowService.ShowAuthorizationWindowDialog();
        }

        async Task<AccessData?> RefreshAccessToken(string refreshToken)
        {
            if (clientId == null)
            {
                throw new InvalidOperationException("Client Id is not set");
            }

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, tokenEndpoint);
            var body = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };
            var content = new FormUrlEncodedContent(body);
            httpRequestMessage.Content = content;
            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            else
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                return responseDictionary == null
                    ? null
                    : new AccessData
                    {
                        AccessToken = responseDictionary["access_token"].ToString(),
                        RefreshToken = refreshToken,
                        ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0"),
                        TokenType = responseDictionary["token_type"].ToString()
                    };
            }
        }

        async Task ISpotifyService.RequestAccessToken(string codeVerifier, string code)
        {
            if (clientId == null)
            {
                throw new InvalidOperationException("Client Id is not set");
            }
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, tokenEndpoint);
            var body = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            };

            var content = new FormUrlEncodedContent(body);
            httpRequestMessage.Content = content;
            var response = await httpClient.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            m_AccessData = responseDictionary == null
                ? null
                : new AccessData
                {
                    AccessToken = responseDictionary["access_token"].ToString(),
                    RefreshToken = responseDictionary["refresh_token"].ToString(),
                    ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0"),
                    TokenType = responseDictionary["token_type"].ToString()
                };
            if (m_AccessData != null && m_AccessData.RefreshToken != null)
                m_PreferenceService.SetRefreshToken(m_AccessData.RefreshToken);
        }
        string ISpotifyService.GetRequestUrl(string codeVerifier)
        {
            string codeChallenge = HashString(codeVerifier);
            string state = ISpotifyService.GenerateRandomString(16);
            string scope = "user-read-private user-read-email user-library-read user-read-playback-state user-modify-playback-state";
            string responseType = "code";
            string url = $"{autorizationEndpoint}?client_id={clientId}&response_type={responseType}&redirect_uri={redirectUri}&code_challenge_method=S256&code_challenge={codeChallenge}&state={state}&scope={scope}";

            return url;
        }
        #endregion

        #region Devices
        async Task<Device?> ISpotifyService.GetLastListenedDevice(string accessToken)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, devicesEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            Device? result = null;
            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(responseString);
                if (responseDictionary != null)
                {
                    var devices = responseDictionary["devices"];
                    if (devices != null)
                    {
                        var device = devices.FirstOrDefault();
                        if (device != null)
                        {
                            var deviceDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(device.ToString() ?? "");
                            if (deviceDictionary != null)
                            {
                                result = new Device
                                {
                                    Id = deviceDictionary["id"].ToString(),
                                    Name = deviceDictionary["name"].ToString() ?? string.Empty,
                                    Type = deviceDictionary["type"].ToString() ?? string.Empty,
                                    IsActive = deviceDictionary["is_active"].ToString() == "True",
                                    IsRestricted = deviceDictionary["is_restricted"].ToString() == "True",
                                    VolumePercent = int.Parse(deviceDictionary["volume_percent"].ToString() ?? "0"),
                                };
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region Playback State
        async Task<PlaybackState> ISpotifyService.GetPlaybackState()
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, playbackStateEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

            var response = await httpClient.SendAsync(httpRequestMessage);
            var result = new PlaybackState() { IsPlaying = false, CurrentlyPlaying = string.Empty, CurrentlyPlayingAlbum = new(), CurrentlyPlayingArtist = string.Empty };
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // No active devices, get the device
                var device = await ((ISpotifyService)this).GetLastListenedDevice(m_AccessData?.AccessToken ?? string.Empty);
                result.DeviceId = device?.Id;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                if (responseDictionary != null)
                {
                    result.IsPlaying = responseDictionary["is_playing"]?.ToString() == "True";
                    result.SetProgress(int.Parse(responseDictionary["progress_ms"]?.ToString() ?? "0"));   

                    var device = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["device"]?.ToString() ?? "");
                    result.DeviceId = device?["id"]?.ToString() ?? string.Empty;
                    if (result.IsPlaying)
                    {
                        try
                        {
                            var item = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["item"]?.ToString() ?? "");
                            result.CurrentlyPlaying = item?["name"]?.ToString() ?? string.Empty;
                            result.DurationMs = int.Parse(item?["duration_ms"]?.ToString() ?? "0");

                            var albumDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(item?["album"]?.ToString() ?? "");
                            result.CurrentlyPlayingAlbum = new Album
                            {
                                Name = albumDictionary?["name"]?.ToString() ?? string.Empty,
                                ImageUrl = JsonSerializer.Deserialize<List<object>>(albumDictionary?["images"]?.ToString() ?? "")?.FirstOrDefault()?.ToString() ?? string.Empty
                            };
                            result.CurrentlyPlayingAlbum = ExtractAlbumData(albumDictionary);


                            var artist = JsonSerializer.Deserialize<List<object>>(item?["artists"].ToString() ?? "")?.First();
                            result.CurrentlyPlayingArtist = (JsonSerializer.Deserialize<Dictionary<string, object>>(artist?.ToString() ?? ""))?["name"]?.ToString() ?? string.Empty;

                            result.SetProgress(int.Parse(responseDictionary["progress_ms"]?.ToString() ?? "0"));
                        }
                        catch (Exception) { }
                    }
                }
            }
            return result;
        }
        async Task<PlaybackState> ISpotifyService.StartPlay(string deviceId)
        {
            var endpoint = playbackStartEndpoint + $"?device_id={deviceId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);


            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState();
            }
            else
            {
                return new PlaybackState
                {
                    IsPlaying = false,
                    CurrentlyPlaying = "Error"

                };
            }
        }

        async Task<PlaybackState> ISpotifyService.PausePlay(string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, playbackPauseEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var body = new Dictionary<string, string>
            {
                { "device_id", deviceId }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;

            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState();
            }
            else
            {
                return new PlaybackState
                {
                    IsPlaying = false,
                    CurrentlyPlaying = "Error"
                };
            }
        }

        async Task<PlaybackState> ISpotifyService.NextTrack(string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, playbackNextEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var body = new Dictionary<string, string>
            {
                { "device_id", deviceId }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;

            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState();
            }
            else
            {
                return new PlaybackState
                {
                    IsPlaying = false,
                    CurrentlyPlaying = "Error"
                };
            }
        }

        async Task<PlaybackState> ISpotifyService.PreviousTrack(string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, playbackPreviousEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var body = new Dictionary<string, string>
            {
                { "device_id", deviceId }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;

            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState();
            }
            else
            {
                return new PlaybackState
                {
                    IsPlaying = false,
                    CurrentlyPlaying = "Error"
                };
            }
        }

        async Task<PlaybackState> ISpotifyService.Seek(string deviceId, int positionMs)
        {
            string endpoint = seekEndpoint + $"?device_id={deviceId}&position_ms={positionMs}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState();
            }
            else
            {
                return new PlaybackState
                {
                    IsPlaying = false,
                    CurrentlyPlaying = "Error"
                };
            }
        }
        #endregion

        #region User
        async Task<User?> ISpotifyService.GetUser()
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, userEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

            var response = await httpClient.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            return responseDictionary == null
                ? null
                : new User
                {
                    Id = responseDictionary["id"].ToString(),
                    DisplayName = responseDictionary["display_name"].ToString(),
                    Email = responseDictionary["email"].ToString(),
                    Country = responseDictionary["country"].ToString(),
                };
        }
        #endregion

        #region Helpers
        internal static string HashString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedInputBytes = System.Security.Cryptography.SHA256.HashData(bytes);
            var converted = Convert.ToBase64String(hashedInputBytes);
            return converted.Replace('+', '-').Replace('/', '_').Replace("=", "").Trim();
        }

        private static Album ExtractAlbumData(Dictionary<string, object>? albumDictionary)
        {
            if (albumDictionary == null) return new();

            var images = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(albumDictionary["images"]?.ToString() ?? string.Empty);
            var imageUrl = images?[0]["url"].ToString() ?? string.Empty;
            return new Album
            {
                Id = albumDictionary["id"]?.ToString() ?? string.Empty,
                Name = albumDictionary["name"]?.ToString() ?? string.Empty,
                ImageUrl = imageUrl
            };
        }
        #endregion

        #region Fields
        private readonly string? clientId;
        private const string redirectUri = "https://mustafacanyucel.com";
        private const string autorizationEndpoint = "https://accounts.spotify.com/authorize";
        private const string tokenEndpoint = "https://accounts.spotify.com/api/token";
        private const string userEndpoint = "https://api.spotify.com/v1/me";
        private const string playbackStateEndpoint = "https://api.spotify.com/v1/me/player";
        private const string playbackStartEndpoint = "https://api.spotify.com/v1/me/player/play";
        private const string playbackPauseEndpoint = "https://api.spotify.com/v1/me/player/pause";
        private const string playbackNextEndpoint = "https://api.spotify.com/v1/me/player/next";
        private const string playbackPreviousEndpoint = "https://api.spotify.com/v1/me/player/previous";
        private const string devicesEndpoint = "https://api.spotify.com/v1/me/player/devices";
        private const string seekEndpoint = "https://api.spotify.com/v1/me/player/seek";
        private const int delay = 500; // ms - delay between consecutive requests
        private readonly HttpClient httpClient = new();
        private readonly IPreferenceService m_PreferenceService;
        private readonly IWindowService m_WindowService;
        private AccessData? m_AccessData;
        #endregion
    }
}
