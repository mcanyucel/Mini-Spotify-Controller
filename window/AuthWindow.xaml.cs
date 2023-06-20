using Microsoft.Web.WebView2.Core;
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
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            string userDataFolder = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\MiniSpotifyController";
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await webView.EnsureCoreWebView2Async(environment);
            webView.CoreWebView2.Navigate(((AuthViewModel)DataContext).RequestUrl);
        }
    }
}
