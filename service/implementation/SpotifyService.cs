﻿using Mini_Spotify_Controller.model;
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
        public SpotifyService(IPreferenceService preferenceService)
        {
            m_PreferenceService = preferenceService;
            clientId = m_PreferenceService.GetClientId();
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }


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
                            var deviceDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(device.ToString());
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

        async Task<PlaybackState> ISpotifyService.GetPlaybackState(string accessToken)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, playbackStateEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(httpRequestMessage);
            var result = new PlaybackState() { IsPlaying = false, CurrentlyPlaying = string.Empty, CurrentlyPlayingAlbum = string.Empty, CurrentlyPlayingArtist = string.Empty };
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                // No active devices, get the device
                var device = await ((ISpotifyService)this).GetLastListenedDevice(accessToken);
                result.DeviceId = device?.Id;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                if (responseDictionary != null)
                {
                    result.IsPlaying = responseDictionary["is_playing"]?.ToString() == "True";
                    var device = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["device"]?.ToString() ?? "");
                    result.DeviceId = device?["id"]?.ToString() ?? string.Empty;
                    if (result.IsPlaying)
                    {
                        try
                        {
                            var item = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["item"]?.ToString() ?? "");
                            result.CurrentlyPlaying = item?["name"]?.ToString() ?? string.Empty;

                            var album = JsonSerializer.Deserialize<Dictionary<string, object>>(item?["album"]?.ToString() ?? "");
                            result.CurrentlyPlayingAlbum = album?["name"]?.ToString() ?? string.Empty;


                            var artist = JsonSerializer.Deserialize<List<object>>(item?["artists"].ToString() ?? "")?.First();
                            result.CurrentlyPlayingArtist = (JsonSerializer.Deserialize<Dictionary<string, object>>(artist?.ToString() ?? ""))?["name"]?.ToString() ?? string.Empty;

                            result.ProgressMs = int.Parse(responseDictionary["progress_ms"]?.ToString() ?? "0");
                        }
                        catch (Exception) { }
                    }
                }
            }

            return result;
        }

        async Task<User?> ISpotifyService.GetUser(string accessToken)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, userEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
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

        async Task<AccessData?> ISpotifyService.RefreshAccessToken(string refreshToken)
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

        async Task<AccessData?> ISpotifyService.RequestAccessToken(string codeVerifier, string code)
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
            var responseDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            return responseDictionary == null
                ? null
                : new AccessData
                {
                    AccessToken = responseDictionary["access_token"].ToString(),
                    RefreshToken = responseDictionary["refresh_token"].ToString(),
                    ExpiresIn = int.Parse(responseDictionary["expires_in"].ToString() ?? "0"),
                    TokenType = responseDictionary["token_type"].ToString()
                };
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

        internal static string HashString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashedInputBytes = hash.ComputeHash(bytes);
                var converted = Convert.ToBase64String(hashedInputBytes);
                return converted.Replace('+', '-').Replace('/', '_').Replace("=", "").Trim();
            }
        }

        async Task<PlaybackState> ISpotifyService.StartPlay(string accessToken, string deviceId)
        {
            var endpoint = playbackStartEndpoint + $"?device_id={deviceId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);


            var response = await httpClient.SendAsync(httpRequestMessage);
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(delay);
                return await ((ISpotifyService)this).GetPlaybackState(accessToken);
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

        async Task<PlaybackState> ISpotifyService.PausePlay(string accessToken, string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, playbackPauseEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
                return await ((ISpotifyService)this).GetPlaybackState(accessToken);
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

        async Task<PlaybackState> ISpotifyService.NextTrack(string accessToken, string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, playbackNextEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
                return await ((ISpotifyService)this).GetPlaybackState(accessToken);
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

        async Task<PlaybackState> ISpotifyService.PreviousTrack(string accessToken, string deviceId)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, playbackPreviousEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
                return await ((ISpotifyService)this).GetPlaybackState(accessToken);
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
        private const int delay = 250; // ms - delay between consecutive requests
        private readonly HttpClient httpClient = new();
        private readonly IPreferenceService m_PreferenceService;
        #endregion




    }
}
