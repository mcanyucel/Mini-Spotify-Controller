using Mini_Spotify_Controller.model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.service
{
    interface ISpotifyService
    {
        internal string GetRequestUrl(string codeVerifier);
        internal Task<AccessData?> RequestAccessToken(string codeVerifier, string code);
        internal Task<AccessData?> RefreshAccessToken(string refreshToken);
        internal Task<PlaybackState> GetPlaybackState(string accessToken);
        internal Task<PlaybackState> StartPlay(string accessToken, string deviceId);
        internal Task<PlaybackState> PausePlay(string accessToken, string deviceId);
        internal Task<PlaybackState> NextTrack(string accessToken, string deviceId);
        internal Task<PlaybackState> PreviousTrack(string accessToken, string deviceId);
        internal Task<User?> GetUser(string accessToken);
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
