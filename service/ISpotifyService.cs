using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSpotifyController.service;

internal interface ISpotifyService
{
    #region Authorization
    internal Task Authorize();
    internal AccessData? AccessData { get; }
    internal bool IsAuthorized { get; }
    internal string GetRequestUrl(string codeVerifier);
    internal Task RequestAccessToken(string codeVerifier, string accessCode);
    #endregion

    #region Playback
    internal event EventHandler<PlaybackState> PlaybackStateChanged;
    internal Task UpdatePlaybackState(PlaybackState? currentState = null);
    internal Task StartPlay(string deviceId);
    internal Task PausePlay(string deviceId);
    internal Task NextTrack(string deviceId);
    internal Task PreviousTrack(string deviceId);
    internal Task Seek(string deviceId, int positionMs);
    #endregion

    #region User
    internal Task<User?> GetUser();
    #endregion

    #region Devices
    internal Task<IEnumerable<Device>> GetDevices();
    internal Task<Device?> GetLastListenedDevice(string accessToken);
    internal Task<bool> TransferPlayback(string deviceId);

    internal const string INTERNAL_PLAYER_NAME = "Mini Spotify Controller";
    #endregion

    #region Track
    internal Task<bool> CheckIfTrackIsSaved(string spotifyId);
    internal Task<bool> SaveTrack(string spotifyId);
    internal Task<bool> RemoveTrack(string spotifyId);
    internal Task<string> GetShareUrl(string spotifyId);
    internal Task<AudioFeatures?> GetAudioFeatures(string spotifyId);
    internal Task<AudioAnalysisResult?> GetAudioAnalysis(string spotifyId);
    #endregion

    #region Radio & Randomization
    internal Task<bool> Randomize(string deviceId);
    internal Task<bool> StartSongRadio(string deviceId, string spotifyId);
    #endregion

    #region Helpers
    internal static string GenerateRandomString(int length)
    {
        Random random = new();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
                         .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    #endregion
}
