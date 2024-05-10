using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.viewmodel;
using System;
using System.Windows.Threading;

namespace MiniSpotifyController.window;

/// <summary>
/// Interaction logic for AudioAnalysisWindow.xaml
/// </summary>
public partial class AudioAnalysisWindow
{
    internal AudioAnalysisWindow()
    {
        viewModel = App.Current.Services.GetRequiredService<AudioAnalysisViewModel>();
        DataContext = viewModel;
        InitializeComponent();
    }

    readonly AudioAnalysisViewModel viewModel;
}
