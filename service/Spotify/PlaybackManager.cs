using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.OAuth;
using Serilog;

namespace MiniSpotifyController.service.Spotify;

public class PlaybackManager(IHttpClientFactory httpClientFactory, OAuthAuthenticator authenticator, TrackManager trackManager, ILogger logger)
{
    public async Task UpdatePlaybackState()
    {
        try
        {
            var httpPlaybackRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, PlaybackStateEndpoint);
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);

            var response = await client.SendAsync(httpPlaybackRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.Error("Failed to get playback state: {responseString}", responseString);
                PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
                return;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var playbackState = ParsePlaybackState(responseString);
                if (playbackState.Track?.Id != null)
                {
                    var isTrackSaved = await trackManager.IsTrackSaved(playbackState.Track.Id);
                    playbackState.IsLiked = isTrackSaved ?? false;
                }
                
                PlaybackStateChanged?.Invoke(this, playbackState);
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to get playback state");
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }

    private PlaybackState ParsePlaybackState(string responseString)
    {
        using var jsonDocument = JsonDocument.Parse(responseString);
        var root = jsonDocument.RootElement;
        var deviceNode = root.GetProperty("device");
        var device = JsonSerializer.Deserialize<Device>(deviceNode.GetRawText()) ??
                     throw new InvalidOperationException("Failed to parse device");
        
        var isPlaying = root.GetProperty("is_playing").GetBoolean();
        
        var itemNode = root.GetProperty("item");
        var track = JsonSerializer.Deserialize<SpotifyTrack>(itemNode.GetRawText()) ??
                    throw new InvalidOperationException("Failed to parse track");
        
        var progressMs = root.GetProperty("progress_ms").GetInt32();

        var playbackState = PlaybackState.CreateState(device, track, isPlaying, progressMs);
        PlayingDevice = device;
        
        return playbackState;
    }

    public async Task Start(Device? device)
    {
        // TODO this nullability is fishy
        if (device?.Id == null) return;
        
        var endPoint = $"{PlaybackStartEndpoint}?device_id={device.Id}";
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        var httpStartRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, endPoint);
        var response = await client.SendAsync(httpStartRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }

    public async Task Pause(Device? device)
    {
        if (device?.Id == null) return;
        
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        var httpPauseRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, PlaybackPauseEndpoint);
        var body = new Dictionary<string, string> { { "device_id", device.Id } };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpPauseRequestMessage.Content = content;
        
        var response = await client.SendAsync(httpPauseRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }

    public async Task Next(Device? device)
    {
        if (device?.Id == null) return;
        
        var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Post, PlaybackNextEndpoint);
        var body = new Dictionary<string, string> { { "device_id", device.Id } };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = content;
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        
        var response = await client.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }
    
    public async Task Previous(Device? device)
    {
        if (device?.Id == null) return;

        var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Post, PlaybackPreviousEndpoint);
        var body = new Dictionary<string, string> { { "device_id", device.Id } };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = content;
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        
        var response = await client.SendAsync(httpRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="position">track position, in milliseconds</param>
    public async Task Seek(Device? device, int position)
    {
        if (device?.Id == null) return;
        
        var endPoint = $"{SeekEndpoint}?device_id={device.Id}&position_ms={position}";
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        var httpSeekRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, endPoint);
        
        var response = await client.SendAsync(httpSeekRequestMessage);
        if (response.IsSuccessStatusCode)
        {
            await Task.Delay(DelayShort);
            await UpdatePlaybackState();
        }
        else
        {
            PlaybackStateChanged?.Invoke(this, PlaybackState.CreatePaused());
        }
    }
    
    public Device? PlayingDevice { get; private set; }
    
    public event EventHandler<PlaybackState>? PlaybackStateChanged;
    private const string PlaybackStateEndpoint = "https://api.spotify.com/v1/me/player";
    private const string PlaybackStartEndpoint = "https://api.spotify.com/v1/me/player/play";
    private const string PlaybackPauseEndpoint = "https://api.spotify.com/v1/me/player/pause";
    private const string PlaybackNextEndpoint = "https://api.spotify.com/v1/me/player/next";
    private const string PlaybackPreviousEndpoint = "https://api.spotify.com/v1/me/player/previous";
    private const string SeekEndpoint = "https://api.spotify.com/v1/me/player/seek";
    private const int DelayShort = 200;
}