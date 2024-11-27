using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.OAuth;
using Serilog;

namespace MiniSpotifyController.service.Spotify;

public class RecommendationManager(IHttpClientFactory httpClientFactory, OAuthAuthenticator authenticator, ILogger logger)
{
    
    public async Task<bool> Randomize(Device device)
    {
        /*
         * Flow
         * 0. Send a dummy request to the saved tracks endpoint to see how many saved tracks the user has.
         * 1. Sample 5 songs with random offsets from the saved tracks endpoint: https://developer.spotify.com/documentation/web-api/reference/library/get-users-saved-tracks/
         * 2. Use the ids of the selected tracks to get recommendations: https://developer.spotify.com/documentation/web-api/reference/browse/get-recommendations/
         * 4. Get max number of recommendations from the recommendation endpoint (100).
         * 5. Start playback of the recommendations with the device id
         */

        try
        {
            List<SpotifyTrack> sampledTracks = [];
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            
            const string dummyRequestEndpoint = $"{SavedTracksEndpoint}?offset=0&limit=1";
            var dummyRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, dummyRequestEndpoint);
            var dummyResponse = await client.SendAsync(dummyRequestMessage);
            dummyResponse.EnsureSuccessStatusCode();
            
            var dummyResponseString = await dummyResponse.Content.ReadAsStringAsync();
            var k = JsonSerializer.Deserialize<JsonElement>(dummyResponseString).GetProperty("total").GetInt32();
            
            if (k<5) return false;
            
            List<int> offsets = [];

            do
            {
                var offset = new Random().Next(0, k);
                while (offsets.Contains(offset))
                {
                    offset = new Random().Next(0, k);
                }
                offsets.Add(offset);
                
                var seedSongEndpoint = $"{SavedTracksEndpoint}?offset={offset}&limit=1";
                var seedSongRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, seedSongEndpoint);
                var response = await client.SendAsync(seedSongRequestMessage);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var trackNode = JsonSerializer.Deserialize<JsonElement>(responseString).GetProperty("items")[0].GetProperty("track");
                var track = JsonSerializer.Deserialize<SpotifyTrack>(trackNode.GetRawText());
                if (track != null)
                {
                    sampledTracks.Add(track);
                } 
            } while (sampledTracks.Count < 5);
            
            var recommendationsUriList = sampledTracks.Select(t => t.Id).ToList();
            var recommendationsEndpoint = $"{RecommendationsEndpoint}?limit=100&seed_tracks={string.Join(",", recommendationsUriList)}";
            var recommendationsRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, recommendationsEndpoint);
            var recommendationsResponse = await client.SendAsync(recommendationsRequestMessage);
            recommendationsResponse.EnsureSuccessStatusCode();
            var recommendationsResponseString = await recommendationsResponse.Content.ReadAsStringAsync();
            var tracksNode = JsonSerializer.Deserialize<JsonElement>(recommendationsResponseString).GetProperty("tracks");
            var recommendations = JsonSerializer.Deserialize<List<SpotifyTrack>>(tracksNode.GetRawText());
            var uris = recommendations?.Select(r => r.Uri).ToList() ?? [];
            
            if (uris.Count == 0) return false;
            
            var playEndpoint = $"{PlaybackStartEndpoint}?device_id={device.Id}";
            var playRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, playEndpoint);
            var body = JsonSerializer.Serialize(new { uris });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            playRequestMessage.Content = content;
            var playResponse = await client.SendAsync(playRequestMessage);
            return playResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to randomize playback");
            return false;
        }
    }

    public async Task<bool> StartSongRadio(Device device, string spotifyId)
    {
        try
        {
            var recommendationEndpoint = $"{RecommendationsEndpoint}?limit={100}&seed_tracks={spotifyId}";
            var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, recommendationEndpoint);
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var response = await client.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var tracksNode = JsonSerializer.Deserialize<JsonElement>(responseString).GetProperty("tracks");
            // ReSharper disable once InvokeAsExtensionMethod
            var recommendations = JsonSerializer.Deserialize<List<SpotifyTrack>>(tracksNode) ?? [];
            var uris = recommendations.Select(r => r.Uri).ToList();
            if (uris.Count == 0) return false;
            
            var playEndpoint = $"{PlaybackStartEndpoint}?device_id={device.Id}";
            var playRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, playEndpoint);
            var body = JsonSerializer.Serialize(new { uris });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            playRequestMessage.Content = content;
            var playResponse = await client.SendAsync(playRequestMessage);
            return playResponse.IsSuccessStatusCode;
            
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to start song radio for track {spotifyId}", spotifyId);
            return false;
        }
    }
    
    private const string RecommendationsEndpoint = "https://api.spotify.com/v1/recommendations";
    private const string PlaybackStartEndpoint = "https://api.spotify.com/v1/me/player/play";
    private const string SavedTracksEndpoint = "https://api.spotify.com/v1/me/tracks";
}