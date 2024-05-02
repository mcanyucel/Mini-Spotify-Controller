using AutoUpdater;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MiniSpotifyController.viewmodel
{
    internal sealed class MainViewModel : ObservableObject, IDisposable
    {
        #region Properties
        public IAsyncRelayCommand AuthorizeCommand { get => m_AutorizeCommand; }
        public IAsyncRelayCommand RefreshCommand { get => m_RefreshCommand; }
        public IRelayCommand TogglePlayCommand { get => m_TogglePlayCommand; }
        public IRelayCommand NextCommand { get => m_NextCommand; }
        public IRelayCommand SeekStartCommand { get => m_SeekStartCommand; }
        public IAsyncRelayCommand<double> SeekEndCommand { get => m_SeekEndCommand; }
        public IAsyncRelayCommand ToggleLikedCommand { get => m_ToggleLikedCommand; }
        public IAsyncRelayCommand GetShareUrlCommand { get => m_GetShareUrlCommand; }
        public IAsyncRelayCommand GetAudioMetricsCommand { get => m_GetAudioMetricsCommand; }
        public IAsyncRelayCommand RandomizeCommand { get => m_RandomizeCommand; }
        public IAsyncRelayCommand StartSongRadioCommand { get => m_StartSongRadioCommand; }
        public IRelayCommand OpenSettingsCommand { get => m_OpenSettingsCommand; }
        public IRelayCommand PreviousCommand { get => m_PreviousCommand; }
        public bool Topmost { get => m_Topmost; set => SetProperty(ref m_Topmost, value); }
        public string? AuthorizationCallbackUrl { get => m_AuthorizationCallbackUrl; set => SetProperty(ref m_AuthorizationCallbackUrl, value); }
        public User? User { get => m_User; set => SetProperty(ref m_User, value); }
        public PlaybackState PlaybackState { get => m_PlaybackState; set { SetProperty(ref m_PlaybackState, value); UpdateCommandStates(); SetTimers(); UpdateMetrics(); } }
        #endregion

        #region Lifecycle
        public MainViewModel(ISpotifyService spotifyService, IToastService toastService, IWindowService windowService)
        {
            m_SpotifyService = spotifyService;
            m_ToastService = toastService;
            m_WindowService = windowService;

            m_AutorizeCommand = new AsyncRelayCommand(Authorize, AuthorizeCanExecute);
            m_TogglePlayCommand = new RelayCommand(TogglePlay, TogglePlayCanExecute);
            m_NextCommand = new RelayCommand(Next, NextCanExecute);
            m_PreviousCommand = new RelayCommand(Previous, PreviousCanExecute);
            m_OpenSettingsCommand = new RelayCommand(OpenSettings);
            m_SeekStartCommand = new RelayCommand(SeekStart);
            m_SeekEndCommand = new AsyncRelayCommand<double>(SeekEnd);
            m_RefreshCommand = new AsyncRelayCommand(Refresh, RefreshCanExecute);
            m_ToggleLikedCommand = new AsyncRelayCommand(ToggleLiked, ToggleLikedCanExecute);
            m_GetShareUrlCommand = new AsyncRelayCommand(GetShareUrl, GetShareUrlCanExecute);
            m_GetAudioMetricsCommand = new AsyncRelayCommand(GetAudioMetrics, GetAudioMetricsCanExecute);
            m_RandomizeCommand = new AsyncRelayCommand(Randomize, RandomizeCanExecute);
            m_StartSongRadioCommand = new AsyncRelayCommand(StartSongRadio, StartSongRadioCanExecute);
            m_AsyncCommandList = new IAsyncRelayCommand[] { m_AutorizeCommand, m_SeekEndCommand, m_RefreshCommand, m_ToggleLikedCommand, m_GetShareUrlCommand, m_GetAudioMetricsCommand, m_RandomizeCommand, m_StartSongRadioCommand }.ToList();
            m_CommandList = new IRelayCommand[] { m_TogglePlayCommand, m_NextCommand, m_PreviousCommand, m_SeekStartCommand, m_OpenSettingsCommand }.ToList();

            m_ProgressTimer = new Timer((object? _) => UpdateProgress(), null, Timeout.Infinite, m_ProgressUpdateInterval);

            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            m_UpdateEngine = new(executingAssemblyName.Name!, executingAssemblyName.Version!.ToString(), "https://software.mustafacanyucel.com/update");
        }
        public void Dispose() => m_ProgressTimer.Dispose();
        #endregion

        #region Authorization Flow
        private async Task Authorize()
        {
            ShowStatus("Status", "Authorizing...");
            await m_SpotifyService.Authorize();

            if (!m_SpotifyService.IsAuthorized)
                m_ToastService.ShowTextToast("error", 0, "Error", "Authorization failed!");
            else
                await OnAuthorizeSuccess();
        }
        private async Task OnAuthorizeSuccess()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                await GetUser();
                PlaybackState = await m_SpotifyService.GetPlaybackState();
                UpdateCommandStates();
            }

            bool hasUpdates = await m_UpdateEngine.CheckForUpdateAsync();
            if (hasUpdates)
            {
                var update = m_WindowService.ShowUpdateWindowDialog();
                if (update == true)
                {
                    var downloaded = await m_UpdateEngine.DownloadAndRunUpdate();
                    if (!downloaded)
                        m_ToastService.ShowTextToast("error", 0, "Error", "Failed to download update!");
                    else
                        Application.Current.Shutdown();
                }
            }
        }
        private async Task GetUser()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                User? user = await m_SpotifyService.GetUser();
                if (user != null)
                    User = user;
                Topmost = true;
            }
        }
        #endregion

        #region Playback State

        private async Task StartSongRadio()
        {
            PlaybackState.IsBusy = true;
            var newPlaybackState = await m_SpotifyService.StartSongRadio(m_PlaybackState.DeviceId!, m_PlaybackState.CurrentlyPlayingId!);
            if (newPlaybackState != null)
                PlaybackState = newPlaybackState;
            else
            {
                ShowError("Error", "Failed to start song radio.");
                PlaybackState.IsBusy = false;
            }
        }
        private async Task Randomize()
        {
            PlaybackState.IsBusy = true;
            var newPlaybackState = await m_SpotifyService.Randomize(m_PlaybackState.DeviceId!);
            if (newPlaybackState != null)
                PlaybackState = newPlaybackState;
            else
            {
                ShowError("Error", "Failed to randomize.");
                PlaybackState.IsBusy = false;
            }
        }
        private void TogglePlay()
        {
            if (m_PlaybackState.IsPlaying)
                Pause();
            else
                Play();
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

        private async Task Refresh()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                PlaybackState = await m_SpotifyService.GetPlaybackState();
                UpdateCommandStates();
            }
        }

        private void SeekStart()
        {
            if (m_PlaybackState.IsPlaying)
                m_IsSeeking = true;
        }

        private async Task SeekEnd(double progressSec)
        {
            if (m_PlaybackState.IsPlaying && m_IsSeeking)
            {
                int progressMs = (int)(progressSec * 1000);
                if (m_SpotifyService.IsAuthorized && m_PlaybackState.DeviceId != null)
                    PlaybackState = await m_SpotifyService.Seek(m_PlaybackState.DeviceId, progressMs);

                m_IsSeeking = false;
            }
        }

        private void UpdateProgress()
        {
            PlaybackState.IncrementProgress(m_ProgressUpdateInterval, m_IsSeeking);

            if (PlaybackState.ProgressMs >= PlaybackState.DurationMs && m_SpotifyService.IsAuthorized)
            {
                PlaybackState.ResetProgress();
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.GetPlaybackState());
            }
        }
        private void UpdateMetrics()
        {
            if (PlaybackState.IsPlaying && m_WindowService.IsAudioMetricsWindowOpen())
                App.Current.Dispatcher.Invoke(() => GetAudioMetricsCommand.Execute(null));
        }
        #endregion

        #region Track Metadata & Sharing
        private async Task ToggleLiked()
        {
            var oldValue = m_PlaybackState.IsLiked;
            bool saved;
            if (oldValue)
                saved = await m_SpotifyService.RemoveTrack(m_PlaybackState.CurrentlyPlayingId ?? string.Empty);
            else
                saved = await m_SpotifyService.SaveTrack(m_PlaybackState.CurrentlyPlayingId ?? string.Empty);

            if (saved)
                m_PlaybackState.IsLiked = !oldValue;
        }
        private async Task GetAudioMetrics()
        {
            AudioFeatures? audioFeatures = await m_SpotifyService.GetAudioFeatures(m_PlaybackState.CurrentlyPlayingId ?? string.Empty);
            AudioAnalysis? audioAnalysis = new(); //await m_SpotifyService.GetAudioAnalysis(m_PlaybackState.CurrentlyPlayingId ?? string.Empty);

            if (audioAnalysis != null && audioFeatures != null)
            {
                audioFeatures.TrackName = PlaybackState.CurrentlyPlaying ?? string.Empty;
                m_WindowService.ShowAudioMetricsWindow(audioFeatures, audioAnalysis);
            }
            else
                ShowError("Error", "Failed to get audio metrics.");
        }
        private async Task GetShareUrl()
        {
            var url = await m_SpotifyService.GetShareUrl(m_PlaybackState.CurrentlyPlayingId ?? string.Empty);
            if (string.IsNullOrEmpty(url))
                ShowError("Error", "Failed to get share url.");
            else
            {
                m_WindowService.SetClipboardText(url);
                m_ToastService.ShowTextToast("info", 0, "Share URL", "Copied to clipboard");
            }
        }
        #endregion

        #region Internal Configuration
        private void SetTimers()
        {
            if (m_PlaybackState.IsPlaying)
            {
                m_ProgressTimer.Change(0, m_ProgressUpdateInterval);
            }
            else
            {
                m_ProgressTimer.Change(Timeout.Infinite, m_ProgressUpdateInterval);
            }
        }
        #endregion

        #region Command States

        private bool StartSongRadioCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.CurrentlyPlayingId != null;
        private bool RandomizeCanExecute() => m_SpotifyService.IsAuthorized;
        private bool GetAudioMetricsCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        private bool GetShareUrlCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        private bool RefreshCanExecute() => m_SpotifyService.IsAuthorized;
        private bool AuthorizeCanExecute() => true;
        private bool TogglePlayCanExecute() => m_SpotifyService.IsAuthorized;
        private bool NextCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        private bool PreviousCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        private bool ToggleLikedCanExecute() => m_SpotifyService.IsAuthorized && m_PlaybackState.IsPlaying;
        private void UpdateCommandStates()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                m_AsyncCommandList.ForEach(x => x.NotifyCanExecuteChanged());
                m_CommandList.ForEach(x => x.NotifyCanExecuteChanged());
            });
        }
        #endregion

        #region UI Helpers
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
        #endregion

        #region Fields
        private readonly ISpotifyService m_SpotifyService;
        private readonly IToastService m_ToastService;
        private readonly IWindowService m_WindowService;
        private readonly IAsyncRelayCommand m_AutorizeCommand;
        private readonly IRelayCommand m_TogglePlayCommand;
        private readonly IRelayCommand m_NextCommand;
        private readonly IRelayCommand m_PreviousCommand;
        private readonly IRelayCommand m_OpenSettingsCommand;
        private readonly IRelayCommand m_SeekStartCommand;
        private readonly IAsyncRelayCommand<double> m_SeekEndCommand;
        private readonly IAsyncRelayCommand m_RefreshCommand;
        private readonly IAsyncRelayCommand m_ToggleLikedCommand;
        private readonly IAsyncRelayCommand m_GetShareUrlCommand;
        private readonly IAsyncRelayCommand m_GetAudioMetricsCommand;
        private readonly IAsyncRelayCommand m_RandomizeCommand;
        private readonly IAsyncRelayCommand m_StartSongRadioCommand;
        private readonly List<IAsyncRelayCommand> m_AsyncCommandList;
        private readonly List<IRelayCommand> m_CommandList;
        private readonly Timer m_ProgressTimer;
        private string? m_AuthorizationCallbackUrl;
        private PlaybackState m_PlaybackState = new();
        private bool m_IsSeeking;
        private bool m_Topmost;

        private const int m_ProgressUpdateInterval = 1000;
        private User? m_User;

        private readonly UpdateEngine m_UpdateEngine;
        #endregion
    }
}
