using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniSpotifyController.model.AudioFeature;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.OAuth;
using MiniSpotifyController.service.Spotify;
using PlaybackState = MiniSpotifyController.model.Spotify.PlaybackState;
namespace MiniSpotifyController.service.implementation;

internal sealed partial class SpotifyService(
    OAuthAuthenticator authenticator,
    DeviceManager deviceManager,
    TrackManager trackManager,
    PlaybackManager playbackManager,
    AudioManager audioManager,
    RecommendationManager recommendationManager,
    UserManager userManager) : ISpotifyService,
    IDisposable
{
    public string AccessToken => authenticator.AccessToken ?? string.Empty;
    

    #region Authorization
    public async Task<bool> Authorize() => await authenticator.Authorize();
    public async Task<bool> IsAuthorized() => await authenticator.IsAuthorized();

    #endregion

    #region Devices
    public async Task<Device?> GetLastListenedDevice() => await deviceManager.GetLastListenedDevice();

    public async Task<IEnumerable<Device>> GetDevices() => await deviceManager.GetDevices();

    public async Task<bool> TransferPlayback(Device device) => await deviceManager.TransferPlayback(device);
    
    public Device? PlayingDevice => playbackManager.PlayingDevice;
    #endregion

    #region Playback State
    
    public event EventHandler<PlaybackState> PlaybackStateChanged
    {
        add
        {
            lock (_eventLock)
            {
                playbackManager.PlaybackStateChanged += value;
            }
        }

        remove
        {
            lock (_eventLock)
            {
                playbackManager.PlaybackStateChanged -= value;
            }
        }
    }

    public async Task<bool> StartSongRadio(string spotifyId, Device? device)
    {
        var targetDevice = device ?? PlayingDevice;
        if (targetDevice?.Id == null)
            return false;
        
        if (!await recommendationManager.StartSongRadio(targetDevice, spotifyId))
            return false;
        
        await Task.Delay(DelayShort);
        await playbackManager.UpdatePlaybackState();
    
        return true;
    }
    
    public async Task<bool> Randomize(Device? device)
    {
        var targetDevice = device ?? PlayingDevice;
        if (targetDevice?.Id == null)
            return false;

        if (!await recommendationManager.Randomize(targetDevice))
            return false;
        
        await Task.Delay(DelayShort);
        
        await playbackManager.UpdatePlaybackState();
        return true;
    }

    public async Task UpdatePlaybackState(PlaybackState? currentState) => await playbackManager.UpdatePlaybackState();
    public async Task StartPlayback(Device? device) {
        if (device?.Id != null)
        {
            await playbackManager.Start(device);
        }
        else
        {
            if (PlayingDevice?.Id != null)
            {
                await playbackManager.Start(PlayingDevice);
            }
        }
    }
    public async Task PausePlayback() => await playbackManager.Pause(PlayingDevice);
    public async Task NextTrack() => await playbackManager.Next(PlayingDevice);
    public async Task PreviousTrack() => await playbackManager.Previous(PlayingDevice);
    public async Task Seek(int position) => await playbackManager.Seek(PlayingDevice, position);
    
    #endregion

    #region Track Data
    public async Task<AudioFeatures?> GetAudioFeatures(string spotifyId) => await audioManager.GetAudioFeatures(spotifyId);
    public async Task<AudioAnalysisResult?> GetAudioAnalysis(string spotifyId) => await audioManager.GetAudioAnalysis(spotifyId);
    public async Task<string?> GetShareUrl(string spotifyId) => await trackManager.GetShareUrl(spotifyId);
    
    public async Task<bool?> IsTrackSaved(string spotifyId) => await trackManager.IsTrackSaved(spotifyId);
    public async Task<bool?> SaveTrack(string spotifyId) => await trackManager.SaveTrack(spotifyId);
    public async Task<bool?> RemoveTrack(string spotifyId) => await trackManager.RemoveTrack(spotifyId);
    #endregion

    #region User

    public async Task<User?> GetUser() => await userManager.GetUser();
    #endregion

    #region Fields
    private readonly object _eventLock = new();
    private bool _disposedValue;
    private const int DelayShort = 500;
    #endregion
    
    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            // dispose managed state (managed objects)
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
