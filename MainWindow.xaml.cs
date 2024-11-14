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
            _viewModel = (MainViewModel)DataContext;
        }

        private async void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                // Define environment for WebView2
                var userDataFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\MiniSpotifyController";
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await webView.EnsureCoreWebView2Async(environment);


                // handle messages from the player so that we can transfer playback once the player is ready
                webView.CoreWebView2.WebMessageReceived += (_, coreWebView2WebMessageReceivedEventArgs) =>
                {
                    var message = coreWebView2WebMessageReceivedEventArgs.TryGetWebMessageAsString();
                    var parts = message.Split('|');
                    if (parts is ["deviceId", _])
                        _viewModel.InternalPlayerId = parts[1];
                    else
                        _viewModel.ShowError("Internal Player Error", "Failed to initialize internal player, it will be disabled.");
                };

                // hook to `InternalPlayerHTMLPath` property change to initialize the internal player once the path is set
                _viewModel.PropertyChanged += (_, propertyChangedEventArgs) =>
                {
                    if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.InternalPlayerHTMLPath))
                        InitializeInternalPlayer();
                };
            }
            catch (Exception)
            {
                _viewModel.ShowError("Internal Player Error", "Failed to initialize webview, internal player will be disabled.");
            }
        }

        void InitializeInternalPlayer()
        {
            try
            {
                // Set up virtual host for WebView2 since EME requires HTTPS
                var htmlFolder = System.IO.Path.GetDirectoryName(_viewModel.InternalPlayerHTMLPath);
                var playerHTMLName = System.IO.Path.GetFileName(_viewModel.InternalPlayerHTMLPath);

                // Update UI elements on the main thread
                Dispatcher.Invoke(() =>
                {
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(VirtualHostName, htmlFolder, CoreWebView2HostResourceAccessKind.Deny);
                    // Navigate to the player HTML
                    webView.CoreWebView2.Navigate($"https://{VirtualHostName}/{playerHTMLName}");
                });

            }
            catch (Exception)
            {
                _viewModel.ShowError("Internal Player Errror", "Failed to initialize internal player; it will be disabled");
            }
        }

        private readonly MainViewModel _viewModel;
        private const string VirtualHostName = "mscplayer";
    }
}
