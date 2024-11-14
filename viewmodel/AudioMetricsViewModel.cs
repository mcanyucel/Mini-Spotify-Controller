using CommunityToolkit.Mvvm.ComponentModel;
using MiniSpotifyController.model;

namespace MiniSpotifyController.viewmodel;

internal sealed partial class AudioMetricsViewModel : ObservableObject, IViewModel
{
    [ObservableProperty] private AudioFeatures? _audioFeatures;

    public void UpdateData(AudioFeatures audioFeatures) => AudioFeatures = audioFeatures;
}
