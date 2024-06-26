﻿using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniSpotifyController.service.implementation;

internal sealed class SpotifyService : ISpotifyService, IDisposable
{
    #region Properties
    AccessData? ISpotifyService.AccessData => m_AccessData;
    bool ISpotifyService.IsAuthorized => m_AccessData != null && m_AccessData.AccessToken != null;

    internal static int DELAY_SHORT => 500;
    internal static int DELAY_LONG => 1500;
    #endregion

    #region Lifecycle
    public SpotifyService(IPreferenceService preferenceService, IWindowService windowService, ILogService logService)
    {
        m_PreferenceService = preferenceService;
        m_WindowService = windowService;
        clientId = m_PreferenceService.GetClientId();
        m_LogService = logService;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
    #endregion

    #region Authorization
    /**
     * Authorization Flow:
     * 1. Client Id is acquired during construction of this class.
     * 2. 
     *    a. If the client id is not set, the user is prompted to enter it. It is a dialog, so it will suspend the flow until the user enters the client id. 
     *    b. If the client id is set, go to step 3.
     * 3. Refresh access token using the refresh token
     *    a. If the access token is successfully refreshed,set m_AccessData and go to step 5.
     *    b. If the access token is not successfully refreshed, go to step 4.
     * 4. Show the access token request dialog. This is a dialog, so it will suspend the flow until the logs in, allows the access, and the access token is acquired. Note tjat tje AuthViewModel will call
     *    RequestAccessToken method with the code verifier and the access code, and this method will set m_AccessData. Then, go to step 5.
     * 5. The access token is successfully refreshed or acquired, and the m_AccessData is set. The flow is complete.
     */
    async Task ISpotifyService.Authorize()
    {
        if (clientId == null)
        {
            m_WindowService.ShowClientIdWindowDialog();
            RefreshClientId();
        }

        m_AccessData = await RefreshAccessToken();
        // If we still don't have an access token, we need to request one
        if (m_AccessData == null)
            ShowAccessTokenRequest();
    }

    void RefreshClientId() => clientId = m_PreferenceService.GetClientId();

    async Task ISpotifyService.RequestAccessToken(string codeVerifier, string code)
    {
        try
        {
            if (clientId == null) throw new InvalidOperationException("Client Id is not set");

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
                    ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0", CultureInfo.InvariantCulture),
                    TokenType = responseDictionary["token_type"].ToString()
                };
            if (m_AccessData != null && m_AccessData.RefreshToken != null)
                m_PreferenceService.SetRefreshToken(m_AccessData.RefreshToken);
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to request access token: {ex.Message}");
            m_AccessData = null;
        }
    }
    string ISpotifyService.GetRequestUrl(string codeVerifier)
    {
        string codeChallenge = HashString(codeVerifier);
        string state = ISpotifyService.GenerateRandomString(16);
        string scope = "user-read-private user-read-email user-library-read user-library-modify user-read-playback-state user-modify-playback-state app-remote-control streaming";
        string responseType = "code";
        string url = $"{autorizationEndpoint}?client_id={clientId}&response_type={responseType}&redirect_uri={redirectUri}&code_challenge_method=S256&code_challenge={codeChallenge}&state={state}&scope={scope}";

        return url;
    }
    private async Task<AccessData?> RefreshAccessToken()
    {
        AccessData? result = null;
        try
        {
            string? refreshToken = m_PreferenceService.GetRefreshToken();
            if (refreshToken != null)
            {
                result = await RefreshAccessToken(refreshToken);
            }
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to refresh access token: {ex.Message}");
        }
        return result;
    }

    private void ShowAccessTokenRequest() => m_WindowService.ShowAuthorizationWindowDialog();

    private async Task<AccessData?> RefreshAccessToken(string refreshToken)
    {
        try
        {
            if (clientId == null) throw new InvalidOperationException("Client Id is not set");

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
                var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                return responseDictionary == null
                    ? null
                    : new AccessData
                    {
                        AccessToken = responseDictionary["access_token"].ToString(),
                        RefreshToken = refreshToken,
                        ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0", CultureInfo.InvariantCulture),
                        TokenType = responseDictionary["token_type"].ToString()
                    };
            }
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to refresh access token: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region Devices
    async Task<Device?> ISpotifyService.GetLastListenedDevice(string accessToken)
    {
        var devices = await ((ISpotifyService)this).GetDevices();
        // if there is an active device, return it
        var activeDevice = devices.FirstOrDefault(d => d.IsActive);
        // if there is no active device, return the first device or null if there are no devices
        return activeDevice ?? devices.FirstOrDefault();
    }

    async Task<IEnumerable<Device>> ISpotifyService.GetDevices()
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, devicesEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
        List<Device> result = [];

        // get the list of devices from the Spotify API
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
                    foreach (var device in devices)
                    {
                        var deviceDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(device.ToString() ?? "");
                        if (deviceDictionary != null)
                        {
                            result.Add(new Device(
                                deviceDictionary["id"].ToString() ?? "Unknown",
                                deviceDictionary["is_active"].ToString() == "True",
                                deviceDictionary["is_private_session"].ToString() == "True",
                                deviceDictionary["is_restricted"].ToString() == "True",
                                deviceDictionary["name"].ToString() ?? "Unnamed",
                                deviceDictionary["type"].ToString() ?? "No Type",
                                int.Parse(deviceDictionary["volume_percent"].ToString() ?? "0", CultureInfo.InvariantCulture),
                                deviceDictionary["supports_volume"].ToString() == "True"
                                ));
                        }
                    }
                }
            }
        }

        return result;
    }

    async Task<bool> ISpotifyService.TransferPlayback(string deviceId)
    {
        bool result;
        try
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, transferPlaybackEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var body = new Dictionary<string, string[]>
            {
                { "device_ids", new string[] { deviceId }}
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;
            var response = await httpClient.SendAsync(httpRequestMessage);
            result = response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to transfer playback: {ex.Message}");
            result = false;
        }
        return result;
    }
    #endregion

    #region Playback State

    async Task<bool> ISpotifyService.StartSongRadio(string deviceId, string spotifyId)
    {
        var result = false;
        try
        {
            var recommendationEndpoint = $"{recommendationsEndpoint}?limit={100}&seed_tracks={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, recommendationEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            var recommendationResponseTracks = JsonSerializer.Deserialize<List<object>>(responseDictionary?["tracks"].ToString() ?? "");
            var recommendedUriList = recommendationResponseTracks?.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")?["uri"]).ToList();

            if (recommendedUriList == null || recommendedUriList.Count == 0)
                throw new InvalidOperationException("No recommendations found");

            var playEndpoint = $"{playbackStartEndpoint}?device_id={deviceId}";
            var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
            HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
            playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var playResponse = await httpClient.SendAsync(playRequestMessage);
            playResponse.EnsureSuccessStatusCode();
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
            result = true;
        }
        catch (Exception ex)
        {
            m_LogService?.LogError($"Error randomizing: {ex.Message}");
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
        return result;
    }
    async Task<bool> ISpotifyService.Randomize(string deviceId)
    {
        /**
         * Flow
         * 0. Set randomization upper limit k to 10000
         * 1. Get user's saved tracks with a random offset between 0 and k and a limit of 50: https://developer.spotify.com/documentation/web-api/reference/get-users-saved-tracks
         * 2. If the response is empty, the user does not have this many saved tracks. Halve k and go to step 1. If not empty, go to step 3.
         * 2. If the respnse has more than 5 tracks, select random 5 tracks from the response. If not, select all tracks from the response.
         * 3. Use the ids of the selected tracks to get recommendations: https://developer.spotify.com/documentation/web-api/reference/browse/get-recommendations/
         * 4. Get max number of recommendations from the recommendation endpoint (100).
         * 5. Start playback of the recommendations with the device id
         */
        var result = false;
        try
        {
            var k = 10000;
            List<object>? savedTracks = null;

            do
            {
                var offset = new Random().Next(0, 100);
                var limit = 50;
                var seedSongEndpoint = $"{savedTracksEndpoint}?offset={offset}&limit={limit}";
                HttpRequestMessage savedTracksRequestMessage = new(HttpMethod.Get, seedSongEndpoint);
                savedTracksRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
                var savedTracksResponse = await httpClient.SendAsync(savedTracksRequestMessage);
                savedTracksResponse.EnsureSuccessStatusCode();
                var savedTracksResponseString = await savedTracksResponse.Content.ReadAsStringAsync();
                var savedTracksResponseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(savedTracksResponseString);
                savedTracks = JsonSerializer.Deserialize<List<object>>(savedTracksResponseDictionary?["items"].ToString() ?? "");
                k /= 2;
            } while (savedTracks == null || (savedTracks.Count == 0 && k > 0));

            if (savedTracks == null || savedTracks.Count == 0)
                throw new InvalidOperationException("No saved tracks found");



            var numberOfTracks = savedTracks.Count;
            var seedCount = numberOfTracks > 5 ? 5 : numberOfTracks;
            var selectedTracks = savedTracks.OrderBy(x => Guid.NewGuid()).Take(seedCount).ToList();
            var selectedTrackList = selectedTracks.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")).ToList();
            var selectedTrackInnerList = selectedTrackList.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t?["track"].ToString() ?? "")).ToList();
            var selectedTrackIds = selectedTrackInnerList.Select(t => t?["id"].ToString() ?? "").ToList();


            if (selectedTrackIds == null || selectedTrackIds.Count == 0)
                throw new InvalidOperationException("No track ids found");

            var seedTracksString = $"seed_tracks={string.Join(",", selectedTrackIds)}";

            var recommendationEndpoint = $"{recommendationsEndpoint}?limit={100}&{seedTracksString}";
            HttpRequestMessage recommendationRequestMessage = new(HttpMethod.Get, recommendationEndpoint);
            recommendationRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var recommendationResponse = await httpClient.SendAsync(recommendationRequestMessage);
            recommendationResponse.EnsureSuccessStatusCode();
            var recommendationResponseString = await recommendationResponse.Content.ReadAsStringAsync();
            var recommendationResponseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(recommendationResponseString);
            var recommendationResponseTracks = JsonSerializer.Deserialize<List<object>>(recommendationResponseDictionary?["tracks"].ToString() ?? "");

            var recommendedUriList = recommendationResponseTracks?.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")?["uri"]).ToList();


            if (recommendedUriList == null || recommendedUriList.Count == 0)
                throw new InvalidOperationException("No recommended track ids found");


            var playEndpoint = $"{playbackStartEndpoint}?device_id={deviceId}";
            var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
            HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
            playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var playResponse = await httpClient.SendAsync(playRequestMessage);
            playResponse.EnsureSuccessStatusCode();
            await Task.Delay(DELAY_LONG);
            await ((ISpotifyService)this).UpdatePlaybackState();
            result = true;
        }
        catch (Exception ex)
        {
            m_LogService?.LogError($"Error randomizing: {ex.Message}");
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });

        }
        return result;
    }

    async Task ISpotifyService.UpdatePlaybackState(PlaybackState? currentState)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, playbackStateEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

        var response = await httpClient.SendAsync(httpRequestMessage);
        var result = new PlaybackState() { IsPlaying = false, CurrentlyPlaying = string.Empty, CurrentlyPlayingArtist = string.Empty };

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
                result.SetProgress(int.Parse(responseDictionary["progress_ms"]?.ToString() ?? "0", CultureInfo.InvariantCulture));

                var device = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["device"]?.ToString() ?? "");
                result.DeviceId = device?["id"]?.ToString() ?? string.Empty;
                if (result.IsPlaying)
                {
                    try
                    {
                        var item = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["item"]?.ToString() ?? "");
                        result.CurrentlyPlayingId = item?["id"]?.ToString() ?? string.Empty;
                        result.CurrentlyPlaying = item?["name"]?.ToString() ?? string.Empty;
                        result.DurationMs = int.Parse(item?["duration_ms"]?.ToString() ?? "0", CultureInfo.InvariantCulture);

                        var albumDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(item?["album"]?.ToString() ?? "");
                        result.CurrentlyPlayingAlbum = new Album(albumDictionary?["id"]?.ToString() ?? string.Empty, albumDictionary?["name"]?.ToString() ?? string.Empty, JsonSerializer.Deserialize<List<object>>(albumDictionary?["images"]?.ToString() ?? "")?.FirstOrDefault()?.ToString() ?? string.Empty);

                        result.CurrentlyPlayingAlbum = ExtractAlbumData(albumDictionary);


                        var artist = JsonSerializer.Deserialize<List<object>>(item?["artists"].ToString() ?? "")?.First();
                        result.CurrentlyPlayingArtist = (JsonSerializer.Deserialize<Dictionary<string, object>>(artist?.ToString() ?? ""))?["name"]?.ToString() ?? string.Empty;

                        result.SetProgress(int.Parse(responseDictionary["progress_ms"]?.ToString() ?? "0", CultureInfo.InvariantCulture));

                        result.IsLiked = await ((ISpotifyService)this).CheckIfTrackIsSaved(result.CurrentlyPlayingId);
                    }
                    catch (Exception ex)
                    {
                        m_LogService.LogError($"Failed to get playback state: {ex.Message}");
                    }
                }
            }
        }
        // check if the state has changed
        if (!result.Equals(currentState))
            PlaybackStateChangedEvent?.Invoke(this, result);
    }
    async Task ISpotifyService.StartPlay(string deviceId)
    {
        var endpoint = playbackStartEndpoint + $"?device_id={deviceId}";
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

        var response = await httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    async Task ISpotifyService.PausePlay(string deviceId)
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
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    async Task ISpotifyService.NextTrack(string deviceId)
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
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    async Task ISpotifyService.PreviousTrack(string deviceId)
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
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    async Task ISpotifyService.Seek(string deviceId, int positionMs)
    {
        string endpoint = seekEndpoint + $"?device_id={deviceId}&position_ms={positionMs}";
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);

        var response = await httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DELAY_SHORT);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }

    // Explicit interface implementation required
    event EventHandler<PlaybackState> ISpotifyService.PlaybackStateChanged
    {
        add
        {
            lock (objectLock)
            {
                PlaybackStateChangedEvent += value;
            }
        }

        remove
        {
            lock (objectLock)
            {
                PlaybackStateChangedEvent -= value;
            }
        }
    }

    #endregion

    #region Track Data
    async Task<AudioFeatures?> ISpotifyService.GetAudioFeatures(string spotifyId)
    {
        AudioFeatures? result = null;
        try
        {
            var endpoint = audioFeaturesEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            result = ExtractAudioFeatures(spotifyId, responseDictionary);
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to get audio features: {ex.Message}");
        }
        return result;
    }

    async Task<AudioAnalysisResult?> ISpotifyService.GetAudioAnalysis(string spotifyId)
    {
        AudioAnalysisResult? result = null;
        try
        {
            var endpoint = audioAnalysisEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            result = await Task.Run(() => JsonSerializer.Deserialize<AudioAnalysisResult>(responseString));
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to get audio analysis result: {ex.Message}");
        }
        return result;
    }
    async Task<string> ISpotifyService.GetShareUrl(string spotifyId)
    {
        var result = string.Empty;
        try
        {
            var endpoint = tracksEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = response.Content.ReadAsStringAsync().Result;
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            var urls = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary?["external_urls"].ToString() ?? string.Empty);
            result = urls?["spotify"].ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to get share url: {ex.Message}");
        }

        return result;
    }
    async Task<bool> ISpotifyService.CheckIfTrackIsSaved(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = libraryCheckEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var resultList = JsonSerializer.Deserialize<List<bool>>(responseString);
            result = resultList?.FirstOrDefault() ?? false;
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to check if track is saved: {ex.Message}");
        }
        return result;
    }
    async Task<bool> ISpotifyService.SaveTrack(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = savedTracksEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            result = true;
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to save track: {ex.Message}");
        }
        return result;
    }
    async Task<bool> ISpotifyService.RemoveTrack(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = savedTracksEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            result = true;
        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to remove track: {ex.Message}");
        }
        return result;

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
            : new User(
                responseDictionary["id"]?.ToString() ?? string.Empty,
                responseDictionary["display_name"]?.ToString() ?? string.Empty,
                responseDictionary["email"]?.ToString() ?? string.Empty,
                responseDictionary["country"]?.ToString() ?? string.Empty
                );
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
        if (albumDictionary == null) return Album.Empty;

        var images = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(albumDictionary["images"]?.ToString() ?? string.Empty);
        var imageUrl = images?[0]["url"].ToString() ?? string.Empty;
        return new Album(albumDictionary["id"]?.ToString() ?? string.Empty, albumDictionary["name"]?.ToString() ?? string.Empty, imageUrl);

    }

    private AudioFeatures? ExtractAudioFeatures(string spotifyId, Dictionary<string, object>? audioFeaturesDictionary)
    {
        if (audioFeaturesDictionary == null) return null;

        AudioFeatures? audioFeatures = null;
        try
        {
            audioFeatures = new AudioFeatures(spotifyId);
            audioFeatures.Features.Add(new AudioFeature("Mode", Convert.ToDouble(audioFeaturesDictionary["mode"].ToString(), CultureInfo.InvariantCulture), 0d, 1d, FeatureType.Text));
            audioFeatures.Features.Add(new AudioFeature("Key", Convert.ToDouble(audioFeaturesDictionary["key"].ToString(), CultureInfo.InvariantCulture), 0d, 11d, FeatureType.Text));
            audioFeatures.Features.Add(new AudioFeature("Tempo", Convert.ToDouble(audioFeaturesDictionary["tempo"].ToString(), CultureInfo.InvariantCulture), 0d, 250d));
            audioFeatures.Features.Add(new AudioFeature("TimeSignature", Convert.ToDouble(audioFeaturesDictionary["time_signature"].ToString(), CultureInfo.InvariantCulture), 3d, 7d));
            audioFeatures.Features.Add(new AudioFeature("Danceability", Convert.ToDouble(audioFeaturesDictionary["danceability"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));
            audioFeatures.Features.Add(new AudioFeature("Energy", Convert.ToDouble(audioFeaturesDictionary["energy"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));
            audioFeatures.Features.Add(new AudioFeature("Loudness", Convert.ToDouble(audioFeaturesDictionary["loudness"].ToString(), CultureInfo.InvariantCulture), -60d, 0d));
            audioFeatures.Features.Add(new AudioFeature("Acousticness", Convert.ToDouble(audioFeaturesDictionary["acousticness"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));
            audioFeatures.Features.Add(new AudioFeature("Instrumentalness", Convert.ToDouble(audioFeaturesDictionary["instrumentalness"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));
            audioFeatures.Features.Add(new AudioFeature("Liveness", Convert.ToDouble(audioFeaturesDictionary["liveness"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));
            audioFeatures.Features.Add(new AudioFeature("Valence", Convert.ToDouble(audioFeaturesDictionary["valence"].ToString(), CultureInfo.InvariantCulture), 0d, 1d));

        }
        catch (Exception ex)
        {
            m_LogService.LogError($"Failed to extract audio features: {ex.Message}");
        }
        return audioFeatures;
    }


    #endregion

    #region Fields
    string? clientId;
    const string redirectUri = "https://mustafacanyucel.com";
    const string autorizationEndpoint = "https://accounts.spotify.com/authorize";
    const string tokenEndpoint = "https://accounts.spotify.com/api/token";
    const string userEndpoint = "https://api.spotify.com/v1/me";
    const string playbackStateEndpoint = "https://api.spotify.com/v1/me/player";
    const string playbackStartEndpoint = "https://api.spotify.com/v1/me/player/play";
    const string playbackPauseEndpoint = "https://api.spotify.com/v1/me/player/pause";
    const string playbackNextEndpoint = "https://api.spotify.com/v1/me/player/next";
    const string playbackPreviousEndpoint = "https://api.spotify.com/v1/me/player/previous";
    const string devicesEndpoint = "https://api.spotify.com/v1/me/player/devices";
    const string transferPlaybackEndpoint = "https://api.spotify.com/v1/me/player";
    const string seekEndpoint = "https://api.spotify.com/v1/me/player/seek";
    const string libraryCheckEndpoint = "https://api.spotify.com/v1/me/tracks/contains";
    const string tracksEndpoint = "https://api.spotify.com/v1/tracks";
    const string savedTracksEndpoint = "https://api.spotify.com/v1/me/tracks";
    const string audioFeaturesEndpoint = "https://api.spotify.com/v1/audio-features";
    const string recommendationsEndpoint = "https://api.spotify.com/v1/recommendations";
    const string audioAnalysisEndpoint = "https://api.spotify.com/v1/audio-analysis";
    readonly HttpClient httpClient = new();
    readonly IPreferenceService m_PreferenceService;
    readonly IWindowService m_WindowService;
    readonly ILogService m_LogService;
    readonly object objectLock = new();
    event EventHandler<PlaybackState>? PlaybackStateChangedEvent;
    AccessData? m_AccessData;
    #endregion
}
