using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using MiniSpotifyController.viewmodel;
using System;

namespace MiniSpotifyController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--autoplay-policy=no-user-gesture-required");
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<MainViewModel>();
            viewModel = (MainViewModel)DataContext;
        }

        async void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            try
            {
                // Define environment for WebView2
                var userDataFolder = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\MiniSpotifyController";
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await webView.EnsureCoreWebView2Async(environment);


                // handle messages from the player so that we can transfer playback once the player is ready
                webView.CoreWebView2.WebMessageReceived += (s, e) =>
                {
                    var message = e.TryGetWebMessageAsString();
                    var parts = message.Split('|');
                    if (parts.Length == 2 && parts[0] == "deviceId")
                        viewModel.InternalPlayerId = parts[1];
                    else
                        viewModel.ShowError("Internal Player Error", "Failed to initialize internal player, it will be disabled.");
                };

                // hook to `InternalPlayerHTMLPath` property change to initialize the internal player once the path is set
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(viewModel.InternalPlayerHTMLPath))
                        InitializeInternalPlayer();
                };
            }
            catch (Exception)
            {
                viewModel.ShowError("Internal Player Error", "Failed to initialize webview, internal player will be disabled.");
            }
        }

        void InitializeInternalPlayer()
        {
            try
            {
                // Set up virtual host for WebView2 since EME requires HTTPS
                var htmlFolder = System.IO.Path.GetDirectoryName(viewModel.InternalPlayerHTMLPath);
                var playerHTMLName = System.IO.Path.GetFileName(viewModel.InternalPlayerHTMLPath);

                // Update UI elements on the main thread
                Dispatcher.Invoke(() =>
                {
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(VIRTUAL_HOST_NAME, htmlFolder, CoreWebView2HostResourceAccessKind.Deny);
                    // Navigate to the player HTML
                    webView.CoreWebView2.Navigate($"https://{VIRTUAL_HOST_NAME}/{playerHTMLName}");
                });
                
            }
            catch (Exception)
            {
                viewModel.ShowError("Internal Player Errror", "Failed to initialize internal player; it will be disabled");
            }
        }

        readonly MainViewModel viewModel;
        const string VIRTUAL_HOST_NAME = "mscplayer";
    }
}
