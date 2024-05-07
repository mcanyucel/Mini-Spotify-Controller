using CommunityToolkit.Mvvm.ComponentModel;
using MiniSpotifyController.model.AudioAnalysis;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AudioAnalysisViewModel : ObservableObject
    {
        [ObservableProperty]
        AudioAnalysisResult? audioAnalysisResult;

        public void UpdateData(AudioAnalysisResult audioAnalysisResult) => AudioAnalysisResult = audioAnalysisResult;
    }
}