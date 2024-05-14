using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for LyricsWindow.xaml
    /// </summary>
    public partial class LyricsWindow
    {
        readonly LyricsViewModel viewModel;
        public LyricsWindow()
        {
            viewModel = App.Current.Services.GetRequiredService<LyricsViewModel>();
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
