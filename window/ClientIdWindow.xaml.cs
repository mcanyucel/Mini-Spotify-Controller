using Mini_Spotify_Controller.viewmodel;

namespace Mini_Spotify_Controller.window
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
