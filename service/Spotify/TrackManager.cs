
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.OAuth;
using MiniSpotifyController.model.Spotify;
using Serilog;

namespace MiniSpotifyController.service.Spotify;

public class TrackManager(OAuthAuthenticator authenticator, IHttpClientFactory httpClientFactory, ILogger logger)
{
    public async Task<string?> GetShareUrl(string spotifyId)
    {
        await authenticator.EnsureAuthorized();

        try
        {
            var endpoint = $"{TracksEndpoint}/{spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpTracksRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await client.SendAsync(httpTracksRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false)
            {
                logger.Error("Failed to get share url for track {spotifyId}: {responseString}", spotifyId, responseString);
                return null;
            }

            var track = JsonSerializer.Deserialize<SpotifyTrack>(responseString);
            return track!.ExternalUrls.Spotify;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get share url for track {spotifyId}", spotifyId);
            return null;
        }
    }

    public async Task<bool?> IsTrackSaved(string spotifyId)
    {
        try
        {
            var endPoint = $"{LibraryCheckEndpoint}?ids={spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpLibraryCheckRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, endPoint);
        
            var response = await client.SendAsync(httpLibraryCheckRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.Error("Failed to check if track {spotifyId} is saved: {responseString}", spotifyId, responseString);
                return null;
            }
        
            var result = JsonSerializer.Deserialize<bool[]>(responseString);
            return result?[0];
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to check if track {spotifyId} is saved", spotifyId);
            return null;
        }
    }

    public async Task<bool?> SaveTrack(string spotifyId)
    {
        try
        {
            var endPoint = $"{SavedTracksEndpoint}?ids={spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpSaveRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, endPoint);
            
            var response = await client.SendAsync(httpSaveRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return true;
            
            logger.Error("Failed to save track {spotifyId}: {responseString}", spotifyId, responseString);
            return null;

        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to save track {spotifyId}", spotifyId);
            return null;
        }
    }

    public async Task<bool?> RemoveTrack(string spotifyId)
    {
        try
        {
            var endPoint = $"{SavedTracksEndpoint}?ids={spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpRemoveRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Delete, endPoint);
        
            var response = await client.SendAsync(httpRemoveRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return true;
        
            logger.Error("Failed to remove track {spotifyId}: {responseString}", spotifyId, responseString);
            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to remove track {spotifyId}", spotifyId);
            return null;
        }
    }
    
    private const string TracksEndpoint = "https://api.spotify.com/v1/tracks";
    private const string LibraryCheckEndpoint = "https://api.spotify.com/v1/me/tracks/contains";
    private const string SavedTracksEndpoint = "https://api.spotify.com/v1/me/tracks";
}