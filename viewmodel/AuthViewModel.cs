using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.service;
using System;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AuthViewModel : ObservableObject
    {
        public string RequestUrl { get => requestUrl; }
        public AuthViewModel(ISpotifyService spotifyService, IWindowService windowService, ILogService logService)
        {
            this.spotifyService = spotifyService;
            this.windowService = windowService;
            this.logService = logService;
            codeVerifier = ISpotifyService.GenerateRandomString(128);
            requestUrl = this.spotifyService.GetRequestUrl(codeVerifier);
        }

        [RelayCommand]
        async Task NavigationCompleted(Uri? uri)
        {
            if (uri != null && codeVerifier != null)
            {
                string url = uri?.ToString() ?? string.Empty;
                if (url.StartsWith("https://mustafacanyucel.com", StringComparison.InvariantCulture))
                {
                    string accessCode = url.Split("code=")[1].Split("&")[0];
                    await spotifyService.RequestAccessToken(codeVerifier, accessCode);
                    windowService.CloseAuthorizationWindowDialog();
                }
            }
        }

        //region Fields
        readonly ISpotifyService spotifyService;
        readonly string codeVerifier;
        readonly string requestUrl;
        readonly IWindowService windowService;
        readonly ILogService logService;
        //endregion
    }
}
