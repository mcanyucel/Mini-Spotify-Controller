using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.service;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AudioAnalysisViewModel : ObservableObject
    {
        [ObservableProperty]
        AudioAnalysisResult? audioAnalysisResult;


        [ObservableProperty]
        bool isBusy;

        public PlaybackState? PlaybackState
        {
            get => playbackState;
            set
            {
                SetProperty(ref playbackState, value);
                SeekToSpanCommand.NotifyCanExecuteChanged();
                Task.Run(GetAudioAnalysis);
            }
        }

        [RelayCommand]
        public async Task Initialize()
        {
            PlaybackState = await spotifyService.GetPlaybackState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spanString">It will be the output of `TrackSpanToStringListConverter`</param>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(GetAudioAnalysisCanExecute))]
        async Task SeekToSpan(string spanString)
        {
            if (string.IsNullOrEmpty(spanString)) return;

            // do not update the audio analysis while seeking - prevents flickering and unnecessary requests
            shouldUpdateAudioAnalysis = false;
            var spanStringArray = spanString.Split('-');
            if (spanStringArray.Length == 2)
                // the format of the timespanstring is "mm:ss.fff"
                if (TimeSpan.TryParseExact(spanStringArray[0].Trim(), "mm\\:ss\\.fff", CultureInfo.InvariantCulture, out var start))
                    await spotifyService.Seek(PlaybackState!.DeviceId!, (int)start.TotalMilliseconds);
        }

        async Task GetAudioAnalysis()
        {
            if (!shouldUpdateAudioAnalysis || IsBusy || string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingId)) return;

            IsBusy = true;
            var result = await spotifyService.GetAudioAnalysis(PlaybackState.CurrentlyPlayingId);
            if (result != null)
                AudioAnalysisResult = result;
            else
                toastService.ShowTextToast("status", 0, "Error", "Failed to get audio analysis");
            IsBusy = false;
            shouldUpdateAudioAnalysis = true;
        }

        public AudioAnalysisViewModel(ISpotifyService spotifyService, IToastService toastService)
        {
            this.spotifyService = spotifyService;
            this.toastService = toastService;

            spotifyService.PlaybackStateChanged += SpotifyService_PlaybackStateChanged;
        }

        bool GetAudioAnalysisCanExecute => !IsBusy && !string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingId);

        private void SpotifyService_PlaybackStateChanged(object? _, PlaybackState e)
        {
            PlaybackState = e;
        }

        readonly ISpotifyService spotifyService;
        readonly IToastService toastService;
        PlaybackState? playbackState;
        bool shouldUpdateAudioAnalysis = true;
    }
}