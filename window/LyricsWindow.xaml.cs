using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for LyricsWindow.xaml
    /// </summary>
    public partial class LyricsWindow
    {
        public LyricsWindow()
        {
            var viewModel = App.Current.Services.GetRequiredService<LyricsViewModel>();
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
