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
    internal sealed partial class LyricsViewModel : ObservableObject, IViewModel
    {
        [ObservableProperty] private LyricsResult? _lyricsResult;

        [ObservableProperty] private bool _isBusy;

        private PlaybackState? PlaybackState
        {
            get => _playbackState;
            set
            {
                SetProperty(ref _playbackState, value);
                Task.Run(GetLyrics);
            }
        }

        [RelayCommand]
        private void Initialize() => _spotifyService.UpdatePlaybackState();

        [RelayCommand]
        private void OpenInGoogleSearch()
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
                _toastService.ShowTextToast("status", 0, "Error", "Error opening browser");
            }
        }

        [RelayCommand]
        private void OpenInGenius()
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
                _toastService.ShowTextToast("status", 0, "Error", "Error opening browser");
            }
        }


        private async Task GetLyrics()
        {
            if (string.IsNullOrEmpty(PlaybackState?.CurrentlyPlaying) || string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingArtist)) return;

            IsBusy = true;
            LyricsResult = await _lyricsService.GetLyrics(PlaybackState.CurrentlyPlaying, PlaybackState.CurrentlyPlayingArtist);
            IsBusy = false;
        }

        public LyricsViewModel(ISpotifyService spotifyService, IToastService toastService, ILyricsService lyricsService)
        {
            _spotifyService = spotifyService;
            _toastService = toastService;
            _lyricsService = lyricsService;

            _spotifyService.PlaybackStateChanged += (_, e) =>
            {
                PlaybackState = e;
            };

        }
        #region Fields

        private readonly ISpotifyService _spotifyService;
        private readonly IToastService _toastService;
        private readonly ILyricsService _lyricsService;
        private PlaybackState? _playbackState;
        #endregion
    }
}
