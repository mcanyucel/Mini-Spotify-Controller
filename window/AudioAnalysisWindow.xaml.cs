using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.viewmodel;

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
            viewModel.UpdateData(audioAnalysisResult, trackName);
            DataContext = viewModel;
            InitializeComponent();
        }

        internal void UpdateData(AudioAnalysisResult audioAnalysisResult, string trackName) => viewModel.UpdateData(audioAnalysisResult, trackName);

        readonly AudioAnalysisViewModel viewModel;
    }
}
