using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.model;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window;

/// <summary>
/// Interaction logic for AudioMetricsWindow.xaml
/// </summary>
public partial class AudioMetricsWindow
{
    private readonly AudioMetricsViewModel? _viewModel;
    internal AudioMetricsWindow(AudioFeatures audioFeatures)
    {
        _viewModel = App.Current.Services.GetRequiredService<AudioMetricsViewModel>();
        _viewModel.UpdateData(audioFeatures);
        DataContext = _viewModel;
        InitializeComponent();
    }

    internal void UpdateData(AudioFeatures audioFeatures) => _viewModel?.UpdateData(audioFeatures);
}
