using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.service;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AudioAnalysisViewModel : ObservableObject
    {
        [ObservableProperty]
        AudioAnalysisResult? audioAnalysisResult;

        [ObservableProperty]
        string? trackName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GetAudioAnalysisCommand))]
        bool isBusy;

        public void UpdateData(string? p_TrackName, string? p_trackId)
        {
            TrackName = p_TrackName;
            trackId = p_trackId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spanString">It will be the output of `TrackSpanToStringListConverter`</param>
        /// <returns></returns>
        [RelayCommand]
        async Task SeekToSpan(string spanString)
        {
            var spanStringArray = spanString.Split('-');
            if (spanStringArray.Length == 2)
            {
                var start = TimeSpan.Parse(spanStringArray[0], CultureInfo.InvariantCulture);
                Debug.WriteLine($"Seeking to {start} for {trackId ?? "idk"}");
                await Task.Delay(1000);
            }
        }

        [RelayCommand(CanExecute = nameof(GetAudioAnalysisCanExecute))]
        async Task GetAudioAnalysis()
        {
            IsBusy = true;
            var result = await spotifyService.GetAudioAnalysis(trackId!);
            if (result != null)
                AudioAnalysisResult = result;
            else
                toastService.ShowTextToast("status", 0, "Error", "Failed to get audio analysis");
            IsBusy = false;
        }

        public AudioAnalysisViewModel(ISpotifyService spotifyService, IToastService toastService)
        {
            this.spotifyService = spotifyService;
            this.toastService = toastService;

            spotifyService.PlaybackStateChanged += SpotifyService_PlaybackStateChanged;
        }

        bool GetAudioAnalysisCanExecute => !IsBusy && !string.IsNullOrEmpty(trackId);

        private void SpotifyService_PlaybackStateChanged(object? _, PlaybackState e)
        {
            if (e.IsPlaying)
            {
                TrackName = e.CurrentlyPlaying;
                trackId = e.CurrentlyPlayingId;
            }
            Task.Run(GetAudioAnalysis);
        }

        string? trackId;
        readonly ISpotifyService spotifyService;
        readonly IToastService toastService;
    }
}