using AutoUpdater;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
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
        public bool Topmost { get => topmost; set => SetProperty(ref topmost, value); }
        public string? AuthorizationCallbackUrl { get => authorizationCallbackUrl; set => SetProperty(ref authorizationCallbackUrl, value); }
        public User? User { get => m_User; set => SetProperty(ref m_User, value); }
        public PlaybackState PlaybackState { get => playbackState; set { SetProperty(ref playbackState, value); UpdateCommandStates(); SetTimers(); UpdateMetrics(); } }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ShowDevicesCommand))]
        bool isBusy;

        [ObservableProperty]
        string? internalPlayerHTMLPath;

        /// <summary>
        /// The internal player ID that is used to transfer playback to the internal player. If null, the internal player is not available.
        /// </summary>
        [ObservableProperty]
        string? internalPlayerId;

        #endregion

        #region Lifecycle
        public MainViewModel(ISpotifyService spotifyService, IToastService toastService, IWindowService windowService, IResourceService resourceService)
        {
            m_SpotifyService = spotifyService;
            m_ToastService = toastService;
            m_WindowService = windowService;
            m_ResourceService = resourceService;

            asyncCommandList = [TogglePlayCommand, NextCommand, PreviousCommand, ToggleLikedCommand, RandomizeCommand, GetShareUrlCommand, RefreshCommand, StartSongRadioCommand,
                GetAudioFeaturesCommand, AuthorizeCommand, SeekEndCommand, GetAudioAnalysisCommand];
            commandList = [SeekStartCommand, OpenSettingsCommand, UpdateMetricsCommand, SeekStartCommand];

            progressTimer = new Timer((object? _) => UpdateProgress(), null, Timeout.Infinite, PROGRESS_UPDATE_INTERVAL_MS);

            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            m_UpdateEngine = new(executingAssemblyName.Name!, executingAssemblyName.Version!.ToString(), "https://software.mustafacanyucel.com/update");

            m_SpotifyService.PlaybackStateChanged += (object? sender, PlaybackState state) => PlaybackState = state;
        }
        public void Dispose() => progressTimer.Dispose();
        #endregion

        #region Authorization Flow
        [RelayCommand(CanExecute = nameof(AuthorizeCanExecute))]
        async Task Authorize()
        {
            ShowStatus("Status", "Authorizing...");
            await m_SpotifyService.Authorize();

            if (!m_SpotifyService.IsAuthorized)
                m_ToastService.ShowTextToast("error", 0, "Error", "Authorization failed!");
            else
                await OnAuthorizeSuccess();
        }
        async Task OnAuthorizeSuccess()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                await GetUser();
                PlaybackState = await m_SpotifyService.GetPlaybackState();
                SetInternalPlayerHTMLPath();
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

        async Task GetUser()
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
                if (devices.Any())
                    m_WindowService.ShowDevicesContextMenu(devices.ToArray(), TransferPlayback);
                else
                    ShowError("Error", "No devices found.");
            else
                ShowError("Error", "Failed to get devices.");
            IsBusy = false;
        }

        /// <summary>
        /// Creates the internal player HTML path and sets it to the internalPlayerHTMLPath property in the background.
        /// </summary>
        void SetInternalPlayerHTMLPath()
        {
            Task.Run(() =>
            {
                try
                {
                    InternalPlayerHTMLPath = m_ResourceService.GetWebPlayerPath(m_SpotifyService.AccessData?.AccessToken ?? string.Empty);
                    if (string.IsNullOrEmpty(InternalPlayerHTMLPath))
                        ShowError("Error", "Failed to create internal player.");
                }
                catch (Exception)
                {
                    ShowError("Error", "Failed to create internal player.");
                }
            });

        }

        // The method that is transferred to the context menu to transfer playback to the selected device.
        internal async Task TransferPlayback(string deviceId)
        {
            IsBusy = true;
            var result = await m_SpotifyService.TransferPlayback(deviceId);
            if (!result)
                ShowError("Error", "Failed to transfer playback!");
            IsBusy = false;
        }

        #endregion

        #region Playback State

        [RelayCommand(CanExecute = nameof(StartSongRadioCanExecute))]
        async Task StartSongRadio()
        {
            PlaybackState.IsBusy = true;
            var success = await m_SpotifyService.StartSongRadio(playbackState.DeviceId!, playbackState.CurrentlyPlayingId!);
            if (!success)
                ShowError("Error", "Failed to start song radio.");

            PlaybackState.IsBusy = false;
        }
        [RelayCommand(CanExecute = nameof(RandomizeCanExecute))]
        async Task Randomize()
        {
            PlaybackState.IsBusy = true;
            var success = await m_SpotifyService.Randomize(playbackState.DeviceId!);
            if (!success)
                ShowError("Error", "Failed to randomize.");

            PlaybackState.IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(TogglePlayCanExecute))]
        async Task TogglePlay()
        {
            if (playbackState.IsPlaying)
                await Pause();
            else
                await Play();
        }

        async Task Play()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                await m_SpotifyService.StartPlay(playbackState.DeviceId);
            else if (playbackState.DeviceId == null)
                ShowError("Error", "No active devices, you should start at least one device manually.");
        }

        async Task Pause()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                await m_SpotifyService.PausePlay(playbackState.DeviceId);
        }

        [RelayCommand(CanExecute = nameof(NextCanExecute))]
        async Task Next()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                await m_SpotifyService.NextTrack(playbackState.DeviceId);
        }

        [RelayCommand(CanExecute = nameof(PreviousCanExecute))]
        async Task Previous()
        {
            if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                await m_SpotifyService.PreviousTrack(playbackState.DeviceId);
        }

        [RelayCommand(CanExecute = nameof(RefreshCanExecute))]
        async Task Refresh()
        {
            if (m_SpotifyService.IsAuthorized)
            {
                PlaybackState = await m_SpotifyService.GetPlaybackState();
                UpdateCommandStates();
            }
        }
        [RelayCommand]
        void SeekStart()
        {
            if (playbackState.IsPlaying)
                isSeeking = true;
        }

        [RelayCommand]
        async Task SeekEnd(double progressSec)
        {
            if (playbackState.IsPlaying && isSeeking)
            {
                int progressMs = (int)(progressSec * 1000);
                if (m_SpotifyService.IsAuthorized && playbackState.DeviceId != null)
                    await m_SpotifyService.Seek(playbackState.DeviceId, progressMs);

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
        [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
        void UpdateMetrics()
        {
            if (PlaybackState.IsPlaying && m_WindowService.IsAudioMetricsWindowOpen())
                Task.Run(GetAudioFeatures);
        }
        #endregion

        #region Track Metadata & Sharing
        [RelayCommand(CanExecute = nameof(ToggleLikedCanExecute))]
        async Task ToggleLiked()
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

        [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
        async Task GetAudioFeatures()
        {
            if (playbackState.CurrentlyPlayingId == null) return;

            AudioFeatures? audioFeatures = await m_SpotifyService.GetAudioFeatures(playbackState.CurrentlyPlayingId);
            if (audioFeatures != null)
            {
                audioFeatures.TrackName = PlaybackState.CurrentlyPlaying ?? string.Empty;
                m_WindowService.ShowAudioFeaturesWindow(audioFeatures);
            }
            else
                ShowError("Error", "Failed to get audio metrics.");
        }

        [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
        async Task GetAudioAnalysis()
        {
            if (playbackState.CurrentlyPlayingId == null) return;
            IsBusy = true;
            AudioAnalysisResult? audioAnalysis = await m_SpotifyService.GetAudioAnalysis(playbackState.CurrentlyPlayingId);
            if (audioAnalysis != null)
            {
                m_WindowService.ShowAudioAnalysisWindow(playbackState.CurrentlyPlaying ?? string.Empty, playbackState.CurrentlyPlayingId);
            }
            else
                ShowError("Error", "Failed to get audio analysis.");
            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(GetShareUrlCanExecute))]
        async Task GetShareUrl()
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
        private static bool AuthorizeCanExecute() => true;
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
        internal void ShowError(string title, string message)
        {
            m_ToastService.ShowTextToast("error", 0, title, message);
        }
        private void ShowStatus(string title, string message)
        {
            m_ToastService.ShowTextToast("status", 0, title, message);
        }
        [RelayCommand]
        void OpenSettings() => m_WindowService.ShowClientIdWindowDialog();

        #endregion

        #region Fields
        readonly ISpotifyService m_SpotifyService;
        readonly IToastService m_ToastService;
        readonly IWindowService m_WindowService;
        readonly IResourceService m_ResourceService;
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
