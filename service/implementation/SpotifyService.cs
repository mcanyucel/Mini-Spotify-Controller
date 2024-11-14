using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.OAuth;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.service.implementation;

internal sealed partial class SpotifyService(OAuthAuthenticator authenticator, IPreferenceService preferenceService, IWindowService windowService, ILogService logService) : ISpotifyService, IDisposable
{
    #region Properties
    
    private static int DelayShort => 500;
    private static int DelayLong => 1500;
    #endregion
    public string AccessToken => authenticator.AccessToken ?? string.Empty;

    #region Authorization
    public async Task<bool> Authorize()
    {
        _clientId = preferenceService.GetClientId();
        if (_clientId != null) return await authenticator.Authorize();
        var clientIdAcquired = AcquireClientId();
        return clientIdAcquired && await authenticator.Authorize();
    }
    
    public async Task<bool> IsAuthorized() => await authenticator.IsAuthorized();

    private bool AcquireClientId()
    {
        const int maxRetry = 3;
        var retry = 0;
        while (string.IsNullOrEmpty(_clientId) && retry < maxRetry)
        {
            windowService.ShowWindow<ClientIdViewModel>();
            _clientId = preferenceService.GetClientId();
            retry++;
        }

        if (!string.IsNullOrEmpty(_clientId)) return true;
        logService.LogError("Failed to acquire client id");
        return false;
    }

    #endregion

    #region Devices
    public async Task<Device?> GetLastListenedDevice()
    {
        var devices = await GetDevices();
        // if there is an active device, return it
        var devicesList = devices.ToList();
        var activeDevice = devicesList.FirstOrDefault(d => d.IsActive);
        // if there is no active device, return the first device or null if there are no devices
        return activeDevice ?? devicesList.FirstOrDefault();
    }

    public async Task<IEnumerable<Device>> GetDevices()
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, DevicesEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        List<Device> result = [];

        // get the list of devices from the Spotify API
        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (!response.IsSuccessStatusCode) return result;
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(responseString);
        var devices = responseDictionary?["devices"];
        if (devices == null) return result;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
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

        return result;
    }

    public async Task<bool> TransferPlayback(string deviceId)
    {
        bool result;
        try
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, TransferPlaybackEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var body = new Dictionary<string, string[]>
            {
                { "device_ids", [deviceId] }
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            httpRequestMessage.Content = content;
            var response = await _httpClient.SendAsync(httpRequestMessage);
            result = response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to transfer playback: {ex.Message}");
            result = false;
        }
        return result;
    }
    #endregion

    #region Playback State

    public async Task<bool> StartSongRadio(string deviceId, string spotifyId)
    {
        var result = false;
        try
        {
            var recommendationEndpoint = $"{RecommendationsEndpoint}?limit={100}&seed_tracks={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, recommendationEndpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            var recommendationResponseTracks = JsonSerializer.Deserialize<List<object>>(responseDictionary?["tracks"].ToString() ?? "");
            var recommendedUriList = recommendationResponseTracks?.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")?["uri"]).ToList();

            if (recommendedUriList == null || recommendedUriList.Count == 0)
                throw new InvalidOperationException("No recommendations found");

            var playEndpoint = $"{PlaybackStartEndpoint}?device_id={deviceId}";
            var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
            HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
            playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var playResponse = await _httpClient.SendAsync(playRequestMessage);
            playResponse.EnsureSuccessStatusCode();
            await Task.Delay(DelayShort);
            await ((ISpotifyService)this).UpdatePlaybackState();
            result = true;
        }
        catch (Exception ex)
        {
            logService.LogError($"Error randomizing: {ex.Message}");
            PlaybackStateChangedEvent?.Invoke(this, new PlaybackState
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
        return result;
    }
    public async Task<bool> Randomize(string deviceId)
    {
        /*
         * Flow
         * 0. Set randomization upper limit k to 10000
         * 1. Get user's saved tracks with a random offset between 0 and k and a limit of 50: https://developer.spotify.com/documentation/web-api/reference/get-users-saved-tracks
         * 2. If the response is empty, the user does not have this many saved tracks. Halve k and go to step 1. If not empty, go to step 3.
         * 2. If the response has more than 5 tracks, select random 5 tracks from the response. If not, select all tracks from the response.
         * 3. Use the ids of the selected tracks to get recommendations: https://developer.spotify.com/documentation/web-api/reference/browse/get-recommendations/
         * 4. Get max number of recommendations from the recommendation endpoint (100).
         * 5. Start playback of the recommendations with the device id
         */
        var result = false;
        try
        {
            var k = 10000;
            List<object>? savedTracks;

            do
            {
                var offset = new Random().Next(0, 100);
                const int limit = 50;
                var seedSongEndpoint = $"{SavedTracksEndpoint}?offset={offset}&limit={limit}";
                HttpRequestMessage savedTracksRequestMessage = new(HttpMethod.Get, seedSongEndpoint);
                savedTracksRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                var savedTracksResponse = await _httpClient.SendAsync(savedTracksRequestMessage);
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
            var selectedTracks = savedTracks.OrderBy(_ => Guid.NewGuid()).Take(seedCount).ToList();
            var selectedTrackList = selectedTracks.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")).ToList();
            var selectedTrackInnerList = selectedTrackList.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t?["track"].ToString() ?? "")).ToList();
            var selectedTrackIds = selectedTrackInnerList.Select(t => t?["id"].ToString() ?? "").ToList();


            if (selectedTrackIds == null || selectedTrackIds.Count == 0)
                throw new InvalidOperationException("No track ids found");

            var seedTracksString = $"seed_tracks={string.Join(",", selectedTrackIds)}";

            var recommendationEndpoint = $"{RecommendationsEndpoint}?limit={100}&{seedTracksString}";
            HttpRequestMessage recommendationRequestMessage = new(HttpMethod.Get, recommendationEndpoint);
            recommendationRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var recommendationResponse = await _httpClient.SendAsync(recommendationRequestMessage);
            recommendationResponse.EnsureSuccessStatusCode();
            var recommendationResponseString = await recommendationResponse.Content.ReadAsStringAsync();
            var recommendationResponseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(recommendationResponseString);
            var recommendationResponseTracks = JsonSerializer.Deserialize<List<object>>(recommendationResponseDictionary?["tracks"].ToString() ?? "");

            var recommendedUriList = recommendationResponseTracks?.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ToString() ?? "")?["uri"]).ToList();


            if (recommendedUriList == null || recommendedUriList.Count == 0)
                throw new InvalidOperationException("No recommended track ids found");


            var playEndpoint = $"{PlaybackStartEndpoint}?device_id={deviceId}";
            var body = JsonSerializer.Serialize(new { uris = recommendedUriList });
            HttpRequestMessage playRequestMessage = new(HttpMethod.Put, playEndpoint);
            playRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            playRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var playResponse = await _httpClient.SendAsync(playRequestMessage);
            playResponse.EnsureSuccessStatusCode();
            await Task.Delay(DelayLong);
            await ((ISpotifyService)this).UpdatePlaybackState();
            result = true;
        }
        catch (Exception ex)
        {
            logService.LogError($"Error randomizing: {ex.Message}");
            PlaybackStateChangedEvent?.Invoke(this, new()
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });

        }
        return result;
    }

    public async Task UpdatePlaybackState(PlaybackState? currentState)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, PlaybackStateEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await _httpClient.SendAsync(httpRequestMessage);
        var result = new PlaybackState() { IsPlaying = false, CurrentlyPlaying = string.Empty, CurrentlyPlayingArtist = string.Empty };

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            // No active devices, get the device
            var device = await GetLastListenedDevice();
            result.DeviceId = device?.Id;
        }
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            if (responseDictionary != null)
            {
                result.IsPlaying = responseDictionary["is_playing"].ToString() == "True";
                result.SetProgress(int.Parse(responseDictionary["progress_ms"].ToString() ?? "0", CultureInfo.InvariantCulture));

                var device = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["device"].ToString() ?? "");
                result.DeviceId = device?["id"].ToString() ?? string.Empty;
                if (result.IsPlaying)
                {
                    try
                    {
                        var item = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary["item"].ToString() ?? "");
                        result.CurrentlyPlayingId = item?["id"].ToString() ?? string.Empty;
                        result.CurrentlyPlaying = item?["name"].ToString() ?? string.Empty;
                        result.DurationMs = int.Parse(item?["duration_ms"].ToString() ?? "0", CultureInfo.InvariantCulture);

                        var albumDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(item?["album"].ToString() ?? "");
                        result.CurrentlyPlayingAlbum = new Album(albumDictionary?["id"].ToString() ?? string.Empty, albumDictionary?["name"].ToString() ?? string.Empty, JsonSerializer.Deserialize<List<object>>(albumDictionary?["images"].ToString() ?? "")?.FirstOrDefault()?.ToString() ?? string.Empty);

                        result.CurrentlyPlayingAlbum = ExtractAlbumData(albumDictionary);


                        var artist = JsonSerializer.Deserialize<List<object>>(item?["artists"].ToString() ?? "")?.First();
                        result.CurrentlyPlayingArtist = (JsonSerializer.Deserialize<Dictionary<string, object>>(artist?.ToString() ?? ""))?["name"].ToString() ?? string.Empty;

                        result.SetProgress(int.Parse(responseDictionary["progress_ms"].ToString() ?? "0", CultureInfo.InvariantCulture));

                        result.IsLiked = await CheckIfTrackIsSaved(result.CurrentlyPlayingId);
                    }
                    catch (Exception ex)
                    {
                        logService.LogError($"Failed to get playback state: {ex.Message}");
                    }
                }
            }
        }
        // check if the state has changed
        if (!result.Equals(currentState))
            PlaybackStateChangedEvent?.Invoke(this, result);
    }
    public async Task StartPlay(string deviceId)
    {
        var endpoint = PlaybackStartEndpoint + $"?device_id={deviceId}";
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new PlaybackState
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    public async Task PausePlay(string deviceId)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, PlaybackPauseEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        var body = new Dictionary<string, string>
        {
            { "device_id", deviceId }
        };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = content;

        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
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
    public async Task NextTrack(string deviceId)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, PlaybackNextEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        var body = new Dictionary<string, string>
        {
            { "device_id", deviceId }
        };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = content;

        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await ((ISpotifyService)this).UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChangedEvent?.Invoke(this, new PlaybackState
            {
                IsPlaying = false,
                CurrentlyPlaying = "Error"
            });
        }
    }
    public async Task PreviousTrack(string deviceId)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, PlaybackPreviousEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        var body = new Dictionary<string, string>
        {
            { "device_id", deviceId }
        };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = content;

        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
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
    public async Task Seek(string deviceId, int positionMs)
    {
        var endpoint = SeekEndpoint + $"?device_id={deviceId}&position_ms={positionMs}";
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await _httpClient.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
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
    public event EventHandler<PlaybackState> PlaybackStateChanged
    {
        add
        {
            lock (_objectLock)
            {
                PlaybackStateChangedEvent += value;
            }
        }

        remove
        {
            lock (_objectLock)
            {
                PlaybackStateChangedEvent -= value;
            }
        }
    }

    #endregion

    #region Track Data
    public async Task<AudioFeatures?> GetAudioFeatures(string spotifyId)
    {
        AudioFeatures? result = null;
        try
        {
            var endpoint = AudioFeaturesEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            result = ExtractAudioFeatures(spotifyId, responseDictionary);
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to get audio features: {ex.Message}");
        }
        return result;
    }

    public async Task<AudioAnalysisResult?> GetAudioAnalysis(string spotifyId)
    {
        AudioAnalysisResult? result = null;
        try
        {
            var endpoint = AudioAnalysisEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            result = await Task.Run(() => JsonSerializer.Deserialize<AudioAnalysisResult>(responseString));
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to get audio analysis result: {ex.Message}");
        }
        return result;
    }
    public async Task<string> GetShareUrl(string spotifyId)
    {
        var result = string.Empty;
        try
        {
            var endpoint = TracksEndpoint + $"/{spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = response.Content.ReadAsStringAsync().Result;
            var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            var urls = JsonSerializer.Deserialize<Dictionary<string, object>>(responseDictionary?["external_urls"].ToString() ?? string.Empty);
            result = urls?["spotify"].ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to get share url: {ex.Message}");
        }

        return result;
    }
    public async Task<bool> CheckIfTrackIsSaved(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = LibraryCheckEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var resultList = JsonSerializer.Deserialize<List<bool>>(responseString);
            result = resultList?.FirstOrDefault() ?? false;
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to check if track is saved: {ex.Message}");
        }
        return result;
    }
    public async Task<bool> SaveTrack(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = SavedTracksEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Put, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            result = true;
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to save track: {ex.Message}");
        }
        return result;
    }
    public async Task<bool> RemoveTrack(string spotifyId)
    {
        var result = false;
        try
        {
            var endpoint = SavedTracksEndpoint + $"?ids={spotifyId}";
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, endpoint);
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            result = true;
        }
        catch (Exception ex)
        {
            logService.LogError($"Failed to remove track: {ex.Message}");
        }
        return result;

    }
    #endregion

    #region User
    public async Task<User?> GetUser()
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, UserEndpoint);
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await _httpClient.SendAsync(httpRequestMessage);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
        return responseDictionary == null
            ? null
            : new User(
                responseDictionary["id"].ToString() ?? string.Empty,
                responseDictionary["display_name"].ToString() ?? string.Empty,
                responseDictionary["email"].ToString() ?? string.Empty,
                responseDictionary["country"].ToString() ?? string.Empty
                );
    }
    #endregion

    #region Helpers

    private static string HashString(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashedInputBytes = System.Security.Cryptography.SHA256.HashData(bytes);
        var converted = Convert.ToBase64String(hashedInputBytes);
        return converted.Replace('+', '-').Replace('/', '_').Replace("=", "").Trim();
    }

    private static Album ExtractAlbumData(Dictionary<string, object>? albumDictionary)
    {
        if (albumDictionary == null) return Album.Empty;

        var images = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(albumDictionary["images"].ToString() ?? string.Empty);
        var imageUrl = images?[0]["url"].ToString() ?? string.Empty;
        return new Album(albumDictionary["id"].ToString() ?? string.Empty, albumDictionary["name"].ToString() ?? string.Empty, imageUrl);

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
            logService.LogError($"Failed to extract audio features: {ex.Message}");
        }
        return audioFeatures;
    }


    #endregion

    #region Fields

    private string? _clientId;
    private const string UserEndpoint = "https://api.spotify.com/v1/me";
    private const string PlaybackStateEndpoint = "https://api.spotify.com/v1/me/player";
    private const string PlaybackStartEndpoint = "https://api.spotify.com/v1/me/player/play";
    private const string PlaybackPauseEndpoint = "https://api.spotify.com/v1/me/player/pause";
    private const string PlaybackNextEndpoint = "https://api.spotify.com/v1/me/player/next";
    private const string PlaybackPreviousEndpoint = "https://api.spotify.com/v1/me/player/previous";
    private const string DevicesEndpoint = "https://api.spotify.com/v1/me/player/devices";
    private const string TransferPlaybackEndpoint = "https://api.spotify.com/v1/me/player";
    private const string SeekEndpoint = "https://api.spotify.com/v1/me/player/seek";
    private const string LibraryCheckEndpoint = "https://api.spotify.com/v1/me/tracks/contains";
    private const string TracksEndpoint = "https://api.spotify.com/v1/tracks";
    private const string SavedTracksEndpoint = "https://api.spotify.com/v1/me/tracks";
    private const string AudioFeaturesEndpoint = "https://api.spotify.com/v1/audio-features";
    private const string RecommendationsEndpoint = "https://api.spotify.com/v1/recommendations";
    private const string AudioAnalysisEndpoint = "https://api.spotify.com/v1/audio-analysis";
    private readonly HttpClient _httpClient = new();
    private readonly object _objectLock = new();
    private event EventHandler<PlaybackState>? PlaybackStateChangedEvent;
    
    #endregion

    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            // dispose managed state (managed objects)
            _httpClient.Dispose();
            authenticator.Dispose();
        }
        // free unmanaged resources (unmanaged objects) and override finalizer
        
        
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
