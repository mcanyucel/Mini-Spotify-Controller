using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.model.Lyrics;
using MiniSpotifyController.service;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class LyricsViewModel : ObservableObject
    {
        [ObservableProperty]
        LyricsResult? lyricsResult;

        [ObservableProperty]
        bool isBusy;

        public PlaybackState? PlaybackState
        {
            get => playbackState;
            set
            {
                SetProperty(ref playbackState, value);
                Task.Run(GetLyrics);
            }
        }

        [RelayCommand]
        public void Initialize() => spotifyService.UpdatePlaybackState();

        [RelayCommand]
        void OpenInGoogleSearch()
        {
            try
            {
                if (string.IsNullOrEmpty(PlaybackState?.CurrentlyPlaying) || string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingArtist)) return;

                var searchQuery = $"{PlaybackState.CurrentlyPlaying} {PlaybackState.CurrentlyPlayingArtist}";
                Process process = new();
                process.StartInfo.FileName = "https://www.google.com/search?q=" + searchQuery;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception)
            {
                toastService.ShowTextToast("status", 0, "Error", "Error opening browser");
            }
        }

        [RelayCommand]
        void OpenInGenius()
        {
            try
            {
                if (string.IsNullOrEmpty(LyricsResult?.GeniusUrl)) return;

                Process process = new();
                process.StartInfo.FileName = LyricsResult.GeniusUrl;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
            catch (Exception)
            {
                toastService.ShowTextToast("status", 0, "Error", "Error opening browser");
            }
        }


        async Task GetLyrics()
        {
            if (string.IsNullOrEmpty(PlaybackState?.CurrentlyPlaying) || string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingArtist)) return;

            IsBusy = true;
            LyricsResult = await lyricsService.GetLyrics(PlaybackState.CurrentlyPlaying, PlaybackState.CurrentlyPlayingArtist);
            IsBusy = false;
        }

        public LyricsViewModel(ISpotifyService spotifyService, IToastService toastService, ILyricsService lyricsService)
        {
            this.spotifyService = spotifyService;
            this.toastService = toastService;
            this.lyricsService = lyricsService;

            this.spotifyService.PlaybackStateChanged += (_, e) =>
            {
                PlaybackState = e;
            };

        }
        #region Fields
        readonly ISpotifyService spotifyService;
        readonly IToastService toastService;
        readonly ILyricsService lyricsService;
        PlaybackState? playbackState;
        #endregion
    }
}
