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
        public IRelayCommand TogglePlayCommand { get => m_TogglePlayCommand; }
        public IRelayCommand NextCommand { get => m_NextCommand; }
        public IRelayCommand OpenSettingsCommand { get => m_OpenSettingsCommand; }
        public IRelayCommand PreviousCommand { get => m_PreviousCommand; }
        public string? AuthorizationCallbackUrl { get => m_AuthorizationCallbackUrl; set => SetProperty(ref m_AuthorizationCallbackUrl, value); }
        public User User { get => m_User; set => SetProperty(ref m_User, value); }
        public PlaybackState PlaybackState { get => m_PlaybackState; set { SetProperty(ref m_PlaybackState, value); UpdateCommandStates(); SetTimers(); } }

        public MainViewModel(ISpotifyService spotifyService, IToastService toastService, IPreferenceService preferenceService, IWindowService windowService)
        {
            m_SpotifyService = spotifyService;
            m_ToastService = toastService;
            m_PreferenceService = preferenceService;
            m_WindowService = windowService;

            m_AutorizeCommand = new AsyncRelayCommand(Authorize, AuthorizeCanExecute);
            m_TogglePlayCommand = new RelayCommand(TogglePlay, TogglePlayCanExecute);
            m_NextCommand = new RelayCommand(Next, NextCanExecute);
            m_PreviousCommand = new RelayCommand(Previous, PreviousCanExecute);
            m_OpenSettingsCommand = new RelayCommand(OpenSettings);
            m_AsyncCommandList = new IAsyncRelayCommand[] { m_AutorizeCommand }.ToList();
            m_CommandList = new IRelayCommand[] { m_TogglePlayCommand, m_NextCommand, m_PreviousCommand }.ToList();

            m_StatusTimer = new Timer(async (object? _) => await UpdateStatus(), null, Timeout.Infinite, m_StatusUpdateInterval);
            m_ProgressTimer = new Timer((object? _) => UpdateProgress(), null, Timeout.Infinite, m_ProgressUpdateInterval);

        }

        private void ShowError(string title, string message)
        {
            m_ToastService.ShowTextToast("error", 0, title, message);
        }

        private void ShowStatus(string title, string message)
        {
            m_ToastService.ShowTextToast("status", 0, title, message);
        }

        private void OpenSettings()
        {
            m_WindowService.ShowClientIdWindowDialog();
        }

        private async Task UpdateStatus()
        {
            if (m_SpotifyService.IsAuthorized)
                PlaybackState = await m_SpotifyService.GetPlaybackState();
        }

        private void SetTimers()
        {
            if (m_PlaybackState.IsPlaying)
            {
                //m_StatusTimer.Change(0, m_StatusUpdateInterval);
                m_ProgressTimer.Change(0, m_ProgressUpdateInterval);
            }
            else
            {
                //m_StatusTimer.Change(Timeout.Infinite, m_StatusUpdateInterval);
                m_ProgressTimer.Change(Timeout.Infinite, m_ProgressUpdateInterval);
            }
        }

        private void UpdateProgress()
        {
            PlaybackState.IncrementProgress(m_ProgressUpdateInterval);
            
            if (PlaybackState.ProgressMs >= PlaybackState.DurationMs && m_SpotifyService.IsAuthorized)
            {
                PlaybackState.ResetProgress();
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.GetPlaybackState());
            }
            // For some reason the OnPropertyChanged is not called when the progress is updated
            OnPropertyChanged(nameof(PlaybackState));
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
            if (m_SpotifyService.IsAuthorized && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.StartPlay(m_PlaybackState.DeviceId));
            else if (m_PlaybackState.DeviceId == null)
                ShowError("Error", "No active devices, you should start at least one device manually.");
        }

        private void Pause()
        {
            if (m_SpotifyService.IsAuthorized && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PausePlay(m_PlaybackState.DeviceId));
        }

        private void Next()
        {
            if (m_SpotifyService.IsAuthorized && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.NextTrack(m_PlaybackState.DeviceId));
        }

        private void Previous()
        {
            if (m_SpotifyService.IsAuthorized && m_PlaybackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PreviousTrack(m_PlaybackState.DeviceId));
        }

        private bool TogglePlayCanExecute()
        {
            return m_SpotifyService.IsAuthorized;
        }

        private bool NextCanExecute()
        {
            return m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        }

        private bool PreviousCanExecute()
        {
            return m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        }

        private async Task OnAuthorizeSuccess()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                await GetUser();
                PlaybackState = await m_SpotifyService.GetPlaybackState();
                UpdateCommandStates();
            }
        }

        private async Task GetUser()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                User? user = await m_SpotifyService.GetUser();
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

        private async Task Authorize()
        {
            // if there is no client id, ask before authorizing
            var clientId = m_PreferenceService.GetClientId();
            if (clientId == null)
            {
                m_WindowService.ShowClientIdWindowDialog();
            }

            ShowStatus("Status", "Authorizing...");
            await m_SpotifyService.Authorize();

            if (!m_SpotifyService.IsAuthorized)
                m_ToastService.ShowTextToast("error", 0, "Error", "Authorization failed!");
            else
                await OnAuthorizeSuccess();
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
            m_ProgressTimer.Dispose();
        }

        private readonly ISpotifyService m_SpotifyService;
        private readonly IToastService m_ToastService;
        private readonly IPreferenceService m_PreferenceService;
        private readonly IWindowService m_WindowService;
        private readonly IAsyncRelayCommand m_AutorizeCommand;
        private readonly IRelayCommand m_TogglePlayCommand;
        private readonly IRelayCommand m_NextCommand;
        private readonly IRelayCommand m_PreviousCommand;
        private readonly IRelayCommand m_OpenSettingsCommand;
        private readonly List<IAsyncRelayCommand> m_AsyncCommandList;
        private readonly List<IRelayCommand> m_CommandList;
        private readonly Timer m_StatusTimer;
        private readonly Timer m_ProgressTimer;
        private string? m_AuthorizationCallbackUrl;
        private PlaybackState m_PlaybackState = new();

        private const int m_ProgressUpdateInterval = 1000;
        private const int m_StatusUpdateInterval = 10000;

        private User m_User = new()
        {
            Id = "",
            DisplayName = "Guest",
            Email = ""
        };
    }
}
