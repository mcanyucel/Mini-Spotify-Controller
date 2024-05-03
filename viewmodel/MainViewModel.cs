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
    internal sealed partial class MainViewModel : ObservableObject, IDisposable
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
        public bool Topmost { get => topmost; set => SetProperty(ref topmost, value); }
        public string? AuthorizationCallbackUrl { get => authorizationCallbackUrl; set => SetProperty(ref authorizationCallbackUrl, value); }
        public User? User { get => m_User; set => SetProperty(ref m_User, value); }
        public PlaybackState PlaybackState { get => playbackState; set { SetProperty(ref playbackState, value); UpdateCommandStates(); SetTimers(); UpdateMetrics(); } }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ShowDevicesCommand))]
        bool isBusy;
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
            asyncCommandList = new IAsyncRelayCommand[] { m_AutorizeCommand, m_SeekEndCommand, m_RefreshCommand, m_ToggleLikedCommand, m_GetShareUrlCommand, m_GetAudioMetricsCommand, m_RandomizeCommand, m_StartSongRadioCommand }.ToList();
            commandList = new IRelayCommand[] { m_TogglePlayCommand, m_NextCommand, m_PreviousCommand, m_SeekStartCommand, m_OpenSettingsCommand }.ToList();

            progressTimer = new Timer((object? _) => UpdateProgress(), null, Timeout.Infinite, PROGRESS_UPDATE_INTERVAL_MS);

            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            m_UpdateEngine = new(executingAssemblyName.Name!, executingAssemblyName.Version!.ToString(), "https://software.mustafacanyucel.com/update");
        }
        public void Dispose() => progressTimer.Dispose();
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

        #region Devices

        [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
        async Task ShowDevices()
        {
            IsBusy = true;
            var devices = await m_SpotifyService.GetDevices();
            if (devices != null)
                m_WindowService.ShowDevicesContextMenu(devices.ToArray(), TransferPlayback);
            else
                ShowError("Error", "Failed to get devices.");
            IsBusy = false;
        }

        private async Task TransferPlayback(string deviceId)
        {
            IsBusy = true;
            var result = await m_SpotifyService.TransferPlayback(deviceId);
            if (!result)
                ShowError("Error", "Failed to transfer playback!");
            IsBusy = false;
        }

        #endregion

        #region Playback State

        private async Task StartSongRadio()
        {
            PlaybackState.IsBusy = true;
            var newPlaybackState = await m_SpotifyService.StartSongRadio(playbackState.DeviceId!, playbackState.CurrentlyPlayingId!);
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
            var newPlaybackState = await m_SpotifyService.Randomize(playbackState.DeviceId!);
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
            if (playbackState.IsPlaying)
                Pause();
            else
                Play();
        }

        private void Play()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.StartPlay(playbackState.DeviceId));
            else if (playbackState.DeviceId == null)
                ShowError("Error", "No active devices, you should start at least one device manually.");
        }

        private void Pause()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PausePlay(playbackState.DeviceId));
        }

        private void Next()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.NextTrack(playbackState.DeviceId));
        }

        private void Previous()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                _ = Task.Run(async () => PlaybackState = await m_SpotifyService.PreviousTrack(playbackState.DeviceId));
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
            if (playbackState.IsPlaying)
                isSeeking = true;
        }

        private async Task SeekEnd(double progressSec)
        {
            if (playbackState.IsPlaying && isSeeking)
            {
                int progressMs = (int)(progressSec * 1000);
                if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                    PlaybackState = await m_SpotifyService.Seek(playbackState.DeviceId, progressMs);

                isSeeking = false;
            }
        }

        private void UpdateProgress()
        {
            PlaybackState.IncrementProgress(PROGRESS_UPDATE_INTERVAL_MS, isSeeking);

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
            var oldValue = playbackState.IsLiked;
            bool saved;
            if (oldValue)
                saved = await m_SpotifyService.RemoveTrack(playbackState.CurrentlyPlayingId ?? string.Empty);
            else
                saved = await m_SpotifyService.SaveTrack(playbackState.CurrentlyPlayingId ?? string.Empty);

            if (saved)
                playbackState.IsLiked = !oldValue;
        }
        private async Task GetAudioMetrics()
        {
            AudioFeatures? audioFeatures = await m_SpotifyService.GetAudioFeatures(playbackState.CurrentlyPlayingId ?? string.Empty);
            AudioAnalysis? audioAnalysis = new(); //await spotifyService.GetAudioAnalysis(playbackState.CurrentlyPlayingId ?? string.Empty);

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
            var url = await m_SpotifyService.GetShareUrl(playbackState.CurrentlyPlayingId ?? string.Empty);
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
            if (playbackState.IsPlaying)
            {
                progressTimer.Change(0, PROGRESS_UPDATE_INTERVAL_MS);
            }
            else
            {
                progressTimer.Change(Timeout.Infinite, PROGRESS_UPDATE_INTERVAL_MS);
            }
        }
        #endregion

        #region Command States

        private bool IsBusyCanExecute() => !IsBusy;

        private bool StartSongRadioCanExecute() => m_SpotifyService.IsAuthorized && playbackState.CurrentlyPlayingId != null;
        private bool RandomizeCanExecute() => m_SpotifyService.IsAuthorized;
        private bool GetAudioMetricsCanExecute() => m_SpotifyService.IsAuthorized && playbackState.IsPlaying;
        private bool GetShareUrlCanExecute() => m_SpotifyService.IsAuthorized && playbackState.IsPlaying;
        private bool RefreshCanExecute() => m_SpotifyService.IsAuthorized;
        private bool AuthorizeCanExecute() => true;
        private bool TogglePlayCanExecute() => m_SpotifyService.IsAuthorized;
        private bool NextCanExecute() => m_SpotifyService.IsAuthorized && playbackState.IsPlaying;
        private bool PreviousCanExecute() => m_SpotifyService.IsAuthorized && playbackState.IsPlaying;
        private bool ToggleLikedCanExecute() => m_SpotifyService.IsAuthorized && playbackState.IsPlaying;
        private void UpdateCommandStates()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                asyncCommandList.ForEach(x => x.NotifyCanExecuteChanged());
                commandList.ForEach(x => x.NotifyCanExecuteChanged());
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
        readonly ISpotifyService m_SpotifyService;
        readonly IToastService m_ToastService;
        readonly IWindowService m_WindowService;
        readonly IAsyncRelayCommand m_AutorizeCommand;
        readonly IRelayCommand m_TogglePlayCommand;
        readonly IRelayCommand m_NextCommand;
        readonly IRelayCommand m_PreviousCommand;
        readonly IRelayCommand m_OpenSettingsCommand;
        readonly IRelayCommand m_SeekStartCommand;
        readonly IAsyncRelayCommand<double> m_SeekEndCommand;
        readonly IAsyncRelayCommand m_RefreshCommand;
        readonly IAsyncRelayCommand m_ToggleLikedCommand;
        readonly IAsyncRelayCommand m_GetShareUrlCommand;
        readonly IAsyncRelayCommand m_GetAudioMetricsCommand;
        readonly IAsyncRelayCommand m_RandomizeCommand;
        readonly IAsyncRelayCommand m_StartSongRadioCommand;
        readonly List<IAsyncRelayCommand> asyncCommandList;
        readonly List<IRelayCommand> commandList;
        readonly Timer progressTimer;
        string? authorizationCallbackUrl;
        PlaybackState playbackState = new();
        bool isSeeking;
        bool topmost;

        const int PROGRESS_UPDATE_INTERVAL_MS = 1000;
        User? m_User;

        readonly UpdateEngine m_UpdateEngine;
        #endregion
    }
}
