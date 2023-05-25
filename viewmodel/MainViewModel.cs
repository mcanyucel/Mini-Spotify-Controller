using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mini_Spotify_Controller.model;
using Mini_Spotify_Controller.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mini_Spotify_Controller.viewmodel
{
    class MainViewModel : ObservableObject, IDisposable
    {

        public IAsyncRelayCommand AuthorizeCommand { get => m_AutorizeCommand; }
        public IAsyncRelayCommand<Uri> NavigationCompletedCommand { get => m_NavigationCompletedCommand; }
        public IRelayCommand TogglePlayCommand { get => m_TogglePlayCommand; }
        public IRelayCommand NextCommand { get => m_NextCommand; }
        public IRelayCommand PreviousCommand { get => m_PreviousCommand; }
        public string? AuthorizationCallbackUrl { get => m_AuthorizationCallbackUrl; set => SetProperty(ref m_AuthorizationCallbackUrl, value); }
        public User User { get => m_User; set => SetProperty(ref m_User, value); }
        public PlaybackState PlaybackState { get => m_PlaybackState; set { SetProperty(ref m_PlaybackState, value); UpdateCommandStates(); } }

        public MainViewModel(ISpotifyService spotifyService, IToastService toastService, IPreferenceService preferenceService, IWindowService windowService)
        {
            m_SpotifyService = spotifyService;
            m_ToastService = toastService;
            m_PreferenceService = preferenceService;
            m_WindowService = windowService;

            m_AutorizeCommand = new AsyncRelayCommand(Authorize, AuthorizeCanExecute);
            m_NavigationCompletedCommand = new AsyncRelayCommand<Uri>(NavigationCompleted);
            m_TogglePlayCommand = new RelayCommand(TogglePlay, TogglePlayCanExecute);
            m_NextCommand = new RelayCommand(Next, NextCanExecute);
            m_PreviousCommand = new RelayCommand(Previous, PreviousCanExecute);
            m_AsyncCommandList = new IAsyncRelayCommand[] { m_AutorizeCommand, m_NavigationCompletedCommand }.ToList();
            m_CommandList = new IRelayCommand[] { m_TogglePlayCommand, m_NextCommand, m_PreviousCommand }.ToList();

            m_StatusTimer = new Timer(async (object? _) => await UpdateStatus(), null, Timeout.Infinite, 10000);

        }

        private void ShowError(string title, string message)
        {
            m_ToastService.ShowTextToast("error", 0, title, message);
        }

        private void ShowStatus(string title, string message)
        {
            m_ToastService.ShowTextToast("status", 0, title, message);
        }

        private async Task UpdateStatus()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null)
                PlaybackState = await m_SpotifyService.GetPlaybackState(m_AccessData.AccessToken);
        }

        private void TogglePlay()
        {
            if (m_PlaybackState.IsPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void Play()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.StartPlay(m_AccessData!.AccessToken!, m_PlaybackState.DeviceId));
            else if (m_PlaybackState.DeviceId == null)
                ShowError("Error", "No active devices, you should start at least one device manually.");
        }

        private void Pause()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PausePlay(m_AccessData!.AccessToken!, m_PlaybackState.DeviceId));
        }

        private void Next()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.NextTrack(m_AccessData!.AccessToken!, m_PlaybackState.DeviceId));
        }

        private void Previous()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PreviousTrack(m_AccessData!.AccessToken!, m_PlaybackState.DeviceId));
        }

        private bool TogglePlayCanExecute()
        {
            return m_AccessData != null && m_AccessData.AccessToken != null;
        }

        private bool NextCanExecute()
        {
            return m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.IsPlaying;
        }

        private bool PreviousCanExecute()
        {
            return m_AccessData != null && m_AccessData.AccessToken != null && m_PlaybackState.IsPlaying;
        }

        private async Task NavigationCompleted(Uri? obj)
        {
            //if (obj == null || m_CodeVerifier == null)
            //{
            //    Debug.WriteLine("NavigationCompleted: obj or m_CodeVerifier is null");
            //}
            //else
            //{
            //    string url = obj?.ToString() ?? string.Empty;
            //    if (url.StartsWith("https://mustafacanyucel.com"))
            //    {
            //        var code = url.Split("code=")[1].Split("&")[0];
            //        try
            //        {
            //            m_AccessData = await m_SpotifyService.RequestAccessToken(m_CodeVerifier, code);
            //            if (m_AccessData == null || m_AccessData.RefreshToken == null)
            //            {
            //                return;
            //            }
            //            else
            //            {

            //                await OnAuthorizeSuccess();
            //            }
            //            m_AuthWindow?.Close();
            //            m_AuthWindow = null;
            //        }
            //        catch (InvalidOperationException) {
            //            m_ToastService.ShowTextToast("error", 0, "Error", "Client id is not set!");
            //        }
            //    }
            //}
        }

        private async Task OnAuthorizeSuccess()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null)
            {
                m_PreferenceService.SetRefreshToken(m_AccessData!.RefreshToken!);
                await GetUser();
                PlaybackState = await m_SpotifyService.GetPlaybackState(m_AccessData.AccessToken);
                UpdateCommandStates();
                if (PlaybackState.IsPlaying)
                    m_StatusTimer.Change(0, 10000);
            }
        }

        private async Task GetUser()
        {
            if (m_AccessData != null && m_AccessData.AccessToken != null)
            {
                User? user = await m_SpotifyService.GetUser(m_AccessData.AccessToken);
                if (user != null)
                {
                    User = user;
                }
            }
        }


        private bool AuthorizeCanExecute()
        {
            return true;
        }

        private void RequestAccessToken()
        {
            m_WindowService.ShowAuthorizationWindow();
        }

        private async Task Authorize()
        {

            var clientId = m_PreferenceService.GetClientId();
            if (clientId == null)
            {
                m_WindowService.ShowClientIdWindow();
            }


            ShowStatus("Status", "Authorizing...");

            string? refreshToken = m_PreferenceService.GetRefreshToken();
            if (refreshToken == null)
            {
                RequestAccessToken();
            }
            else
            {
                try
                {
                    m_AccessData = await m_SpotifyService.RefreshAccessToken(refreshToken);
                    if (m_AccessData == null || m_AccessData.RefreshToken == null)
                    {
                        // Refresh token has expired/revoked, request new one
                        RequestAccessToken();
                    }
                    else
                    {
                        await OnAuthorizeSuccess();
                    }
                }
                catch (InvalidOperationException)
                {
                    m_ToastService.ShowTextToast("error", 0, "Error", "Client id is not set!");
                }
            }
        }

        private void UpdateCommandStates()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                m_AsyncCommandList.ForEach(x => x.NotifyCanExecuteChanged());
                m_CommandList.ForEach(x => x.NotifyCanExecuteChanged());
            });
        }

        public void Dispose()
        {
            m_StatusTimer.Dispose();
        }

        private readonly ISpotifyService m_SpotifyService;
        private readonly IToastService m_ToastService;
        private readonly IPreferenceService m_PreferenceService;
        private readonly IWindowService m_WindowService;
        private readonly IAsyncRelayCommand m_AutorizeCommand;
        private readonly IAsyncRelayCommand<Uri> m_NavigationCompletedCommand;
        private readonly IRelayCommand m_TogglePlayCommand;
        private readonly IRelayCommand m_NextCommand;
        private readonly IRelayCommand m_PreviousCommand;
        private readonly List<IAsyncRelayCommand> m_AsyncCommandList;
        private readonly List<IRelayCommand> m_CommandList;
        private readonly Timer m_StatusTimer;
        private string? m_AuthorizationCallbackUrl;
        private AccessData? m_AccessData;
        private PlaybackState m_PlaybackState = new();

        private User m_User = new()
        {
            Id = "",
            DisplayName = "Guest",
            Email = ""
        };


    }
}
