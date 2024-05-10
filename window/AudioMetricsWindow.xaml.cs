using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.model;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window;

/// <summary>
/// Interaction logic for AudioMetricsWindow.xaml
/// </summary>
public partial class AudioMetricsWindow
{
    private readonly AudioMetricsViewModel? m_ViewModel;
    internal AudioMetricsWindow(AudioFeatures audioFeatures)
    {
        m_ViewModel = App.Current.Services.GetRequiredService<AudioMetricsViewModel>();
        m_ViewModel.UpdateData(audioFeatures);
        DataContext = m_ViewModel;
        InitializeComponent();
    }

    internal void UpdateData(AudioFeatures audioFeatures) => m_ViewModel?.UpdateData(audioFeatures);
}
