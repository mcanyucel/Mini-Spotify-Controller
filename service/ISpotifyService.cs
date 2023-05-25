﻿using Mini_Spotify_Controller.model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.service
{
    interface ISpotifyService
    {
        internal Task Authorize();
        internal AccessData? AccessData { get; }
        internal bool IsAuthorized { get; }
        internal string GetRequestUrl(string codeVerifier);
        internal Task RequestAccessToken(string codeVerifier, string accessCode);
        internal Task<PlaybackState> GetPlaybackState();
        internal Task<PlaybackState> StartPlay(string deviceId);
        internal Task<PlaybackState> PausePlay(string deviceId);
        internal Task<PlaybackState> NextTrack(string deviceId);
        internal Task<PlaybackState> PreviousTrack(string deviceId);
        internal Task<User?> GetUser();
        internal Task<Device?> GetLastListenedDevice(string accessToken);
        internal static string GenerateRandomString(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                             .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        

        
    }
}
