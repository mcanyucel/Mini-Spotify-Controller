using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.viewmodel;
using System.Windows.Threading;
using System;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for AudioAnalysisWindow.xaml
    /// </summary>
    public partial class AudioAnalysisWindow
    {
        internal AudioAnalysisWindow(AudioAnalysisResult audioAnalysisResult, string trackName)
        {
            viewModel = App.Current.Services.GetRequiredService<AudioAnalysisViewModel>();
            // do not call update data here as it stalls the UI thread significantly
            this.audioAnalysisResult = audioAnalysisResult;
            this.trackName = trackName;
            DataContext = viewModel;
            InitializeComponent();
        }

        internal void UpdateData(AudioAnalysisResult audioAnalysisResult, string trackName) => viewModel.UpdateData(audioAnalysisResult, trackName);

        readonly AudioAnalysisViewModel viewModel;

        readonly AudioAnalysisResult audioAnalysisResult;
        readonly string trackName;

        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
            viewModel.UpdateData(audioAnalysisResult, trackName);
        }
    }
}
