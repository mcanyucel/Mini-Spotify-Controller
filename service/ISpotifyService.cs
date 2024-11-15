using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniSpotifyController.model.AudioFeature;
using MiniSpotifyController.model.Spotify;
using PlaybackState = MiniSpotifyController.model.Spotify.PlaybackState;

namespace MiniSpotifyController.service;

public interface ISpotifyService
{
    #region Authorization
    public Task<bool> Authorize();
    public Task<bool> IsAuthorized();
    public string? AccessToken { get; }
    #endregion

    #region Playback
    internal event EventHandler<PlaybackState> PlaybackStateChanged;
    internal Task UpdatePlaybackState(PlaybackState? currentState = null);
    internal Task StartPlayback(Device? device = null);
    internal Task PausePlayback();
    internal Task NextTrack();
    internal Task PreviousTrack();
    internal Task Seek(int position);
    #endregion

    #region User
    internal Task<User?> GetUser();
    #endregion

    #region Devices
    internal Task<IEnumerable<Device>> GetDevices();
    internal Task<Device?> GetLastListenedDevice();
    internal Task<bool> TransferPlayback(Device device);
    internal Device? PlayingDevice { get; }
    internal const string InternalPlayerName = "Mini Spotify Controller";
    #endregion

    #region Track
    internal Task<bool?> IsTrackSaved(string spotifyId);
    internal Task<bool?> SaveTrack(string spotifyId);
    internal Task<bool?> RemoveTrack(string spotifyId);
    internal Task<string?> GetShareUrl(string spotifyId);
    internal Task<AudioFeatures?> GetAudioFeatures(string spotifyId);
    internal Task<AudioAnalysisResult?> GetAudioAnalysis(string spotifyId);
    #endregion

    #region Radio & Randomization
    internal Task<bool> Randomize(Device? device = null);
    internal Task<bool> StartSongRadio(string spotifyId, Device? device = null);
    #endregion
}
