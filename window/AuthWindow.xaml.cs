using Mini_Spotify_Controller.viewmodel;

namespace Mini_Spotify_Controller.window
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow
    {
        public AuthWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService(typeof(AuthViewModel));
        }
    }
}
