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
        internal AudioAnalysisWindow(AudioAnalysisResult audioAnalysisResult)
        {
            viewModel = App.Current.Services.GetRequiredService<AudioAnalysisViewModel>();
            viewModel.UpdateData(audioAnalysisResult);
            DataContext = viewModel;
            InitializeComponent();
        }

        internal void UpdateData(AudioAnalysisResult audioAnalysisResult) => viewModel.UpdateData(audioAnalysisResult);

        readonly AudioAnalysisViewModel viewModel;
    }
}
