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
                        ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0"),
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
            string scope = "user-read-private user-read-email user-library-read user-library-modify user-read-playback-state user-modify-playback-state";
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
                if (clientId == null) throw new Exception("Client Id is not set");

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
                            ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0"),
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

        async Task<PlaybackState?> ISpotifyService.StartSongRadio(string deviceId, string spotifyId)
        {
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

                if (recommendedUriList == null || !recommendedUriList.Any())
                    throw new Exception("No recommendations found");

                var playEndpoint = $"{playbackStartEndpoint}?device_id={deviceId}";
                var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
                HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
                playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
                playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
                var playResponse = await httpClient.SendAsync(playRequestMessage);
                playResponse.EnsureSuccessStatusCode();
                await Task.Delay(m_LongDelay);
                var newState = await ((ISpotifyService)this).GetPlaybackState();
                return newState;
            }
            catch (Exception ex)
            {
                m_LogService?.LogError($"Error randomizing: {ex.Message}");
                return null;
            }
        }
        async Task<PlaybackState?> ISpotifyService.Randomize(string deviceId)
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

                if (savedTracks == null || !savedTracks.Any())
                    throw new Exception("No saved tracks found");



                var numberOfTracks = savedTracks.Count;
                var seedCount = numberOfTracks > 5 ? 5 : numberOfTracks;
                var selectedTracks = savedTracks.OrderBy(x => Guid.NewGuid()).Take(seedCount).ToList();
                var selectedTrackList = selectedTracks.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")).ToList();
                var selectedTrackInnerList = selectedTrackList.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t?["track"].ToString() ?? "")).ToList();
                var selectedTrackIds = selectedTrackInnerList.Select(t => t?["id"].ToString() ?? "").ToList();


                if (selectedTrackIds == null || !selectedTrackIds.Any())
                    throw new Exception("No track ids found");

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


                if (recommendedUriList == null || !recommendedUriList.Any())
                    throw new Exception("No recommended track ids found");


                var playEndpoint = $"{playbackStartEndpoint}?device_id={deviceId}";
                var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
                HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
                playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", m_AccessData?.AccessToken);
                playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
                var playResponse = await httpClient.SendAsync(playRequestMessage);
                playResponse.EnsureSuccessStatusCode();
                await Task.Delay(m_LongDelay);
                var newState = await ((ISpotifyService)this).GetPlaybackState();
                return newState;
            }
            catch (Exception ex)
            {
                m_LogService?.LogError($"Error randomizing: {ex.Message}");
                return null;
            }
        }

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
                            result.CurrentlyPlayingId = item?["id"]?.ToString() ?? string.Empty;
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

                            result.IsLiked = await ((ISpotifyService)this).CheckIfTrackIsSaved(result.CurrentlyPlayingId);
                        }
                        catch (Exception ex)
                        {
                            m_LogService.LogError($"Failed to get playback state: {ex.Message}");
                        }
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
                await Task.Delay(m_ShortDelay);
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
                await Task.Delay(m_ShortDelay);
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
                await Task.Delay(m_ShortDelay);
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
                await Task.Delay(m_ShortDelay);
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
                await Task.Delay(m_ShortDelay);
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

                var responseString = response.Content.ReadAsStringAsync().Result;
                var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                result = ExtractAudioFeatures(spotifyId, responseDictionary);
            }
            catch (Exception ex)
            {
                m_LogService.LogError($"Failed to get audio features: {ex.Message}");
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

        private AudioFeatures? ExtractAudioFeatures(string spotifyId, Dictionary<string, object>? audioFeaturesDictionary)
        {
            if (audioFeaturesDictionary == null) return null;

            AudioFeatures? audioFeatures = null;
            try
            {
                audioFeatures = new AudioFeatures(spotifyId);
                audioFeatures.Features.Add(new AudioFeature("Mode", Convert.ToDouble(audioFeaturesDictionary["mode"].ToString()), 0d, 1d, FeatureType.Text));
                audioFeatures.Features.Add(new AudioFeature("Key", Convert.ToDouble(audioFeaturesDictionary["key"].ToString()), 0d, 11d, FeatureType.Text));
                audioFeatures.Features.Add(new AudioFeature("Tempo", Convert.ToDouble(audioFeaturesDictionary["tempo"].ToString()), 0d, 250d));
                audioFeatures.Features.Add(new AudioFeature("TimeSignature", Convert.ToDouble(audioFeaturesDictionary["time_signature"].ToString()), 3d, 7d));
                audioFeatures.Features.Add(new AudioFeature("Danceability", Convert.ToDouble(audioFeaturesDictionary["danceability"].ToString()), 0d, 1d));
                audioFeatures.Features.Add(new AudioFeature("Energy", Convert.ToDouble(audioFeaturesDictionary["energy"].ToString()), 0d, 1d));
                audioFeatures.Features.Add(new AudioFeature("Loudness", Convert.ToDouble(audioFeaturesDictionary["loudness"].ToString()), -60d, 0d));
                audioFeatures.Features.Add(new AudioFeature("Acousticness", Convert.ToDouble(audioFeaturesDictionary["acousticness"].ToString()), 0d, 1d));
                audioFeatures.Features.Add(new AudioFeature("Instrumentalness", Convert.ToDouble(audioFeaturesDictionary["instrumentalness"].ToString()), 0d, 1d));
                audioFeatures.Features.Add(new AudioFeature("Liveness", Convert.ToDouble(audioFeaturesDictionary["liveness"].ToString()), 0d, 1d));
                audioFeatures.Features.Add(new AudioFeature("Valence", Convert.ToDouble(audioFeaturesDictionary["valence"].ToString()), 0d, 1d));

            }
            catch (Exception ex)
            {
                m_LogService.LogError($"Failed to extract audio features: {ex.Message}");
            }
            return audioFeatures;
        }
        #endregion

        #region Fields
        private string? clientId;
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
        private const string libraryCheckEndpoint = "https://api.spotify.com/v1/me/tracks/contains";
        private const string tracksEndpoint = "https://api.spotify.com/v1/tracks";
        private const string savedTracksEndpoint = "https://api.spotify.com/v1/me/tracks";
        private const string audioFeaturesEndpoint = "https://api.spotify.com/v1/audio-features";
        private const string recommendationsEndpoint = "https://api.spotify.com/v1/recommendations";
        private const int m_ShortDelay = 500; // ms - a short delay between consecutive requests
        private const int m_LongDelay = 1500; // ms - a long delay between consecutive requests
        private readonly HttpClient httpClient = new();
        private readonly IPreferenceService m_PreferenceService;
        private readonly IWindowService m_WindowService;
        private readonly ILogService m_LogService;
        private AccessData? m_AccessData;
        #endregion
    }
}
