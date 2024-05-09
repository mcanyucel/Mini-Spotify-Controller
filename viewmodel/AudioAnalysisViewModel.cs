using CommunityToolkit.Mvvm.ComponentModel;
using MiniSpotifyController.model.AudioAnalysis;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AudioAnalysisViewModel : ObservableObject
    {
        [ObservableProperty]
        AudioAnalysisResult? audioAnalysisResult;

        [ObservableProperty]
        string? trackName;

        [ObservableProperty]
        string? trackId;

        [ObservableProperty]
        bool isBusy;

        public void UpdateData(AudioAnalysisResult p_AudioAnalysisResult, string p_TrackName)
        {
            AudioAnalysisResult = p_AudioAnalysisResult;
            TrackName = p_TrackName;
        }
    }
}