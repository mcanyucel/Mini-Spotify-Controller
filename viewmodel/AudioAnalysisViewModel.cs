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
        [ObservableProperty] private AudioAnalysisResult? _audioAnalysisResult;


        [ObservableProperty] private bool _isBusy;

        public PlaybackState? PlaybackState
        {
            get => _playbackState;
            private set
            {
                SetProperty(ref _playbackState, value);
                SeekToSpanCommand.NotifyCanExecuteChanged();
                Task.Run(GetAudioAnalysis);
            }
        }

        [RelayCommand]
        private async Task Initialize()
        {
            await _spotifyService.UpdatePlaybackState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spanString">It will be the output of `TrackSpanToStringListConverter`</param>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(GetAudioAnalysisCanExecute))]
        private async Task SeekToSpan(string spanString)
        {
            if (string.IsNullOrEmpty(spanString)) return;

            // do not update the audio analysis while seeking - prevents flickering and unnecessary requests
            _shouldUpdateAudioAnalysis = false;
            var spanStringArray = spanString.Split('-');
            if (spanStringArray.Length == 2)
                // the format of the time spans tring is "mm:ss.fff"
                if (TimeSpan.TryParseExact(spanStringArray[0].Trim(), "mm\\:ss\\.fff", CultureInfo.InvariantCulture, out var start))
                    await _spotifyService.Seek(PlaybackState!.DeviceId!, (int)start.TotalMilliseconds);
        }

        [RelayCommand(CanExecute = nameof(GetAudioAnalysisCanExecute))]
        private async Task SeekToSegment(Segment segment)
        {
            // do not update the audio analysis while seeking - prevents flickering and unnecessary requests
            _shouldUpdateAudioAnalysis = false;
            await _spotifyService.Seek(PlaybackState!.DeviceId!, (int)(segment.Start * 1000));
        }

        [RelayCommand(CanExecute = nameof(GetAudioAnalysisCanExecute))]
        private async Task SeekToSection(Section section)
        {
            // do not update the audio analysis while seeking - prevents flickering and unnecessary requests
            _shouldUpdateAudioAnalysis = false;
            await _spotifyService.Seek(PlaybackState!.DeviceId!, (int)(section.Start * 1000));
        }

        private async Task GetAudioAnalysis()
        {
            if (!_shouldUpdateAudioAnalysis || IsBusy || string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingId))
            {
                _shouldUpdateAudioAnalysis = true;
                return;
            }

            IsBusy = true;
            var result = await _spotifyService.GetAudioAnalysis(PlaybackState.CurrentlyPlayingId);
            if (result != null)
                AudioAnalysisResult = result;
            else
                _toastService.ShowTextToast("status", 0, "Error", "Failed to get audio analysis");
            IsBusy = false;
        }

        public AudioAnalysisViewModel(ISpotifyService spotifyService, IToastService toastService)
        {
            _spotifyService = spotifyService;
            _toastService = toastService;

            _spotifyService.PlaybackStateChanged += SpotifyService_PlaybackStateChanged;
        }

        private bool GetAudioAnalysisCanExecute => !IsBusy && !string.IsNullOrEmpty(PlaybackState?.CurrentlyPlayingId);

        private void SpotifyService_PlaybackStateChanged(object? _, PlaybackState e)
        {
            PlaybackState = e;
        }

        private readonly ISpotifyService _spotifyService;
        private readonly IToastService _toastService;

        private PlaybackState? _playbackState;
        // if false, the next playback state change will not trigger a new audio analysis request (only once per seek)
        private bool _shouldUpdateAudioAnalysis = true;
    }
}