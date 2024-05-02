using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for ClientIdWindow.xaml
    /// </summary>
    public partial class ClientIdWindow
    {
        public ClientIdWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService(typeof(ClientIdViewModel));
        }
    }
}
