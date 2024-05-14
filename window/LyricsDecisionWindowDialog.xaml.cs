using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for LyricsDecisionWindowDialog.xaml
    /// </summary>
    public partial class LyricsDecisionWindowDialog
    {
        public LyricsDecisionWindowDialog()
        {
            DataContext = App.Current.Services.GetRequiredService<MainViewModel>();
            InitializeComponent();
        }
    }
}
