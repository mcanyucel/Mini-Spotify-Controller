using CommunityToolkit.Mvvm.ComponentModel;
using MiniSpotifyController.model;

namespace MiniSpotifyController.viewmodel;

internal sealed partial class AudioMetricsViewModel : ObservableObject
{
    [ObservableProperty]
    AudioFeatures? audioFeatures;
    [ObservableProperty]
    AudioAnalysis? audioAnalysis;
    
    public void UpdateData(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
    {
        AudioAnalysis = audioAnalysis;
        AudioFeatures = audioFeatures;
    }
}
