using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mini_Spotify_Controller.service;
using System;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.viewmodel
{
    class AuthViewModel : ObservableObject
    {
        public IAsyncRelayCommand<Uri?> NavigationCompletedCommand { get => m_NavigationCompletedCommand; }
        public string RequestUrl { get => m_RequestUrl; }
        public AuthViewModel(ISpotifyService spotifyService, IWindowService windowService, ILogService logService)
        {
            m_SpotifyService = spotifyService;
            m_WindowService = windowService;
            m_LogService = logService;
            m_NavigationCompletedCommand = new AsyncRelayCommand<Uri?>(NavigationCompleted);
            m_CodeVerifier = ISpotifyService.GenerateRandomString(128);
            m_RequestUrl = m_SpotifyService.GetRequestUrl(m_CodeVerifier);
        }

        private async Task NavigationCompleted(Uri? uri)
        {
            if (uri != null && m_CodeVerifier != null)
            {
                string url = uri?.ToString() ?? string.Empty;
                if (url.StartsWith("https://mustafacanyucel.com"))
                {
                    string accessCode = url.Split("code=")[1].Split("&")[0];
                    await m_SpotifyService.RequestAccessToken(m_CodeVerifier, accessCode);
                    m_WindowService.CloseAuthorizationWindowDialog();
                }
            }
        }

        //region Fields
        private readonly ISpotifyService m_SpotifyService;
        private readonly string m_CodeVerifier;
        private readonly string m_RequestUrl;
        private readonly IAsyncRelayCommand<Uri?> m_NavigationCompletedCommand;
        private readonly IWindowService m_WindowService;
        private readonly ILogService m_LogService;
        //endregion
    }
}
