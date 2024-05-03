using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using MiniSpotifyController.viewmodel;
using System.Diagnostics;
using System.Windows;

namespace MiniSpotifyController.window
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        public PlayerWindow(string playerHTMLPath)
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<MainViewModel>();
            this.viewModel = (MainViewModel)DataContext;
            this.playerHTMLPath = playerHTMLPath;
        }

        private async void Window_ContentRendered(object sender, System.EventArgs e)
        {
            // Define environment for WebView2
            var userDataFolder = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)}\\MiniSpotifyController";
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            await webView.EnsureCoreWebView2Async(environment);


            // Set up virtual host for WebView2 since EME requires HTTPS
            var htmlFolder = System.IO.Path.GetDirectoryName(playerHTMLPath);
            var playerHTMLName = System.IO.Path.GetFileName(playerHTMLPath);
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(VIRTUAL_HOST_NAME, htmlFolder, CoreWebView2HostResourceAccessKind.DenyCors);

            // Handle messages from the player so that we can transfer playback once the player is ready
            webView.CoreWebView2.WebMessageReceived += async (s, e) =>
            {
                var message = e.TryGetWebMessageAsString();
                var parts = message.Split('|');
                if (parts.Length == 2 && parts[0] == "deviceId")
                {
                    await viewModel.TransferPlayback(parts[1]);
                }
                else
                {
                    viewModel.ShowError("Internal Player Error", "Failed to initialize internal player");
                }
            };
            // Navigate to the player HTML
            webView.CoreWebView2.Navigate($"https://{VIRTUAL_HOST_NAME}/{playerHTMLName}");
        }




        readonly MainViewModel viewModel;
        readonly string playerHTMLPath;
        const string VIRTUAL_HOST_NAME = "mscplayer";
    }
}
