using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.viewmodel;
using System.Windows.Threading;
using System;

namespace MiniSpotifyController.window;

/// <summary>
/// Interaction logic for AudioAnalysisWindow.xaml
/// </summary>
public partial class AudioAnalysisWindow
{
    internal AudioAnalysisWindow(string trackName, string trackId)
    {
        viewModel = App.Current.Services.GetRequiredService<AudioAnalysisViewModel>();
        
        this.trackName = trackName;
        this.trackId = trackId;
        DataContext = viewModel;
        InitializeComponent();
    }

    internal void UpdateData(string trackName, string trackId) => viewModel.UpdateData(trackName, trackId);

    readonly AudioAnalysisViewModel viewModel;

    string? trackName;
    string? trackId;


    private void MetroWindow_ContentRendered(object sender, EventArgs e)
    {
        Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
        viewModel.UpdateData(trackName, trackId);
    }
}
