using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mini_Spotify_Controller.service;

namespace Mini_Spotify_Controller.viewmodel
{
    class AuthViewModel : ObservableObject
    {
        public IRelayCommand NavigationCompletedCommand { get => m_NavigationCompletedCommand; }
        public string RequestUrl { get => m_RequestUrl; }
        public AuthViewModel(ISpotifyService spotifyService, IWindowService windowService)
        {
            m_SpotifyService = spotifyService;
            m_WindowService = windowService;
            m_NavigationCompletedCommand = new RelayCommand(NavigationCompleted);
            m_CodeVerifier = ISpotifyService.GenerateRandomString(128);
            m_RequestUrl = m_SpotifyService.GetRequestUrl(m_CodeVerifier);
        }

        private void NavigationCompleted()
        {

        }

        //region Fields
        private readonly ISpotifyService m_SpotifyService;
        private readonly string m_CodeVerifier;
        private readonly string m_RequestUrl;
        private readonly IRelayCommand m_NavigationCompletedCommand;
        private readonly IWindowService m_WindowService;
        //endregion
    }
}
