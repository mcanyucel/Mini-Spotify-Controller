using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.service;
using System;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class AuthViewModel : ObservableObject
    {
        public string RequestUrl { get; }

        public AuthViewModel(ISpotifyService spotifyService, IWindowService windowService)
        {
            _spotifyService = spotifyService;
            _windowService = windowService;
            _codeVerifier = ISpotifyService.GenerateRandomString(128);
            RequestUrl = _spotifyService.GetRequestUrl(_codeVerifier);
        }

        [RelayCommand]
        private async Task NavigationCompleted(Uri? uri)
        {
            if (uri != null)
            {
                var url = uri.ToString();
                if (url.StartsWith("https://mustafacanyucel.com", StringComparison.InvariantCulture))
                {
                    var accessCode = url.Split("code=")[1].Split("&")[0];
                    await _spotifyService.RequestAccessToken(_codeVerifier, accessCode);
                    _windowService.CloseAuthorizationWindowDialog();
                }
            }
        }

        //region Fields
        private readonly ISpotifyService _spotifyService;
        private readonly string _codeVerifier;
        private readonly IWindowService _windowService;
        //endregion
    }
}
