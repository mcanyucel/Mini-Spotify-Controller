using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model;
using MiniSpotifyController.model.Lyrics;
using MiniSpotifyController.service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniSpotifyController.viewmodel;

internal sealed partial class MainViewModel : ObservableObject, IDisposable, IViewModel
{
    #region Properties
    public bool Topmost { get => _topmost; set => SetProperty(ref _topmost, value); }
    public User? User { get => _user;
        private set => SetProperty(ref _user, value); }
    public PlaybackState PlaybackState { get => _playbackState;
        private set { SetProperty(ref _playbackState, value); UpdateCommandStates(); SetTimers(); UpdateMetrics(); } }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowDevicesCommand))]
    private bool _isBusy;

    // ReSharper disable once InconsistentNaming
    [ObservableProperty] private string? _internalPlayerHTMLPath;

    /// <summary>
    /// The internal player ID that is used to transfer playback to the internal player. If null, the internal player is not available.
    /// </summary>
    [ObservableProperty] private string? _internalPlayerId;

    [ObservableProperty] private LyricsResult? _lyricsResult;

    #endregion

    #region Lifecycle
    public MainViewModel(ISpotifyService spotifyService, IToastService toastService, IWindowService windowService, IResourceService resourceService)
    {
        _spotifyService = spotifyService;
        _toastService = toastService;
        _windowService = windowService;
        _resourceService = resourceService;

        _asyncCommandList = [TogglePlayCommand, NextCommand, PreviousCommand, ToggleLikedCommand, RandomizeCommand, GetShareUrlCommand, RefreshCommand, StartSongRadioCommand,
            GetAudioFeaturesCommand, AuthorizeCommand, SeekEndCommand];
        _commandList = [SeekStartCommand, OpenSettingsCommand, UpdateMetricsCommand, SeekStartCommand, GetAudioAnalysisCommand];

        _progressTimer = new Timer(_ => UpdateProgress(), null, Timeout.Infinite, ProgressUpdateIntervalMs);
        
        _spotifyService.PlaybackStateChanged += (_, state) => PlaybackState = state;
    }
    public void Dispose() => _progressTimer.Dispose();
    #endregion

    #region Authorization Flow
    [RelayCommand(CanExecute = nameof(AuthorizeCanExecute))]
    private async Task Authorize()
    {
        ShowStatus("Status", "Authorizing...");

        var authorized = await _spotifyService.IsAuthorized();
        
        if (!authorized)
            authorized = await _spotifyService.Authorize();
        
        if (!authorized)
            _toastService.ShowTextToast("error", 0, "Error", "Authorization failed!");
        else
            await OnAuthorizeSuccess();
    }

    private async Task OnAuthorizeSuccess()
    {
            await GetUser();
            await _spotifyService.UpdatePlaybackState();
            SetInternalPlayerHtmlPath();
            UpdateCommandStates();
    }

    private async Task GetUser()
    {
        var user = await _spotifyService.GetUser();
        if (user != null)
            User = user;
        Topmost = true;
    }

    #endregion

    #region Devices

    [RelayCommand(CanExecute = nameof(IsBusyCanExecute))]
    private async Task ShowDevices()
    {
        IsBusy = true;
        var devices = await _spotifyService.GetDevices();
        var deviceList = devices as Device[] ?? devices.ToArray();
        if (deviceList.Length != 0)
            _windowService.ShowDevicesContextMenu([.. deviceList], TransferPlayback);
        else
            ShowError("Error", "No devices found.");
        IsBusy = false;
    }

    /// <summary>
    /// Creates the internal player HTML path and sets it to the internalPlayerHTMLPath property in the background.
    /// </summary>
    private void SetInternalPlayerHtmlPath()
    {
        Task.Run(() =>
        {
            try
            {
                InternalPlayerHTMLPath = _resourceService.GetWebPlayerPath(_spotifyService.AccessToken ?? string.Empty);
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
    private async Task TransferPlayback(string deviceId)
    {
        IsBusy = true;
        var result = await _spotifyService.TransferPlayback(deviceId);
        if (!result)
            ShowError("Error", "Failed to transfer playback!");
        IsBusy = false;
    }

    #endregion

    #region Playback State

    [RelayCommand(CanExecute = nameof(StartSongRadioCanExecute))]
    private async Task StartSongRadio()
    {
        PlaybackState.IsBusy = true;
        var success = await _spotifyService.StartSongRadio(_playbackState.DeviceId!, _playbackState.CurrentlyPlayingId!);
        if (!success)
            ShowError("Error", "Failed to start song radio.");

        PlaybackState.IsBusy = false;
    }

    [RelayCommand]
    private async Task Randomize()
    {
        PlaybackState.IsBusy = true;
        var success = await _spotifyService.Randomize(_playbackState.DeviceId!);
        if (!success)
            ShowError("Error", "Failed to randomize.");

        PlaybackState.IsBusy = false;
    }

    [RelayCommand]
    private async Task TogglePlay()
    {
        if (_playbackState.IsPlaying)
            await Pause();
        else
            await Play();
    }

    
    private async Task Play()
    {
        if (await _spotifyService.IsAuthorized() && _playbackState.DeviceId != null)
            await _spotifyService.StartPlay(_playbackState.DeviceId);
        else if (_playbackState.DeviceId == null)
            ShowError("Error", "No active devices, you should start at least one device manually.");
    }

    
    private async Task Pause()
    {
        if (await _spotifyService.IsAuthorized() && _playbackState.DeviceId != null)
            await _spotifyService.PausePlay(_playbackState.DeviceId);
    }

    [RelayCommand(CanExecute = nameof(NextCanExecute))]
    private async Task Next()
    {
        if (await _spotifyService.IsAuthorized() && _playbackState.DeviceId != null)
            await _spotifyService.NextTrack(_playbackState.DeviceId);
    }

    [RelayCommand(CanExecute = nameof(PreviousCanExecute))]
    private async Task Previous()
    {
        if (await _spotifyService.IsAuthorized() && _playbackState.DeviceId != null)
            await _spotifyService.PreviousTrack(_playbackState.DeviceId);
    }
    
    [RelayCommand]
    private async Task Refresh()
    {
        if (await _spotifyService.IsAuthorized())
        {
            await _spotifyService.UpdatePlaybackState();
            UpdateCommandStates();
        }
    }
    [RelayCommand]
    private void SeekStart()
    {
        if (_playbackState.IsPlaying)
            _isSeeking = true;
    }

    [RelayCommand]
    private async Task SeekEnd(double progressSec)
    {
        if (_playbackState.IsPlaying && _isSeeking)
        {
            var progressMs = (int)(progressSec * 1000);
            if (await _spotifyService.IsAuthorized() && _playbackState.DeviceId != null)
                await _spotifyService.Seek(_playbackState.DeviceId, progressMs);

            _isSeeking = false;
        }
    }

    private void UpdateProgress()
    {
        PlaybackState.IncrementProgress(ProgressUpdateIntervalMs, _isSeeking);

        if (PlaybackState.ProgressMs < PlaybackState.DurationMs) return;
        PlaybackState.ResetProgress();
        _ = Task.Run(async () => await _spotifyService.UpdatePlaybackState());
    }
    [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
    private void UpdateMetrics()
    {

    }
    #endregion

    #region Lyrics

    [RelayCommand]
    private void GetLyrics()
    {
        _windowService.ShowWindow<LyricsViewModel>();
    }
    #endregion

    #region Track Metadata & Sharing
    [RelayCommand(CanExecute = nameof(ToggleLikedCanExecute))]
    private async Task ToggleLiked()
    {
        var oldValue = _playbackState.IsLiked;
        bool saved;
        if (oldValue)
            saved = await _spotifyService.RemoveTrack(_playbackState.CurrentlyPlayingId ?? string.Empty);
        else
            saved = await _spotifyService.SaveTrack(_playbackState.CurrentlyPlayingId ?? string.Empty);

        if (saved)
            _playbackState.IsLiked = !oldValue;
    }

    [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
    private async Task GetAudioFeatures()
    {
        if (_playbackState.CurrentlyPlayingId == null) return;

        var audioFeatures = await _spotifyService.GetAudioFeatures(_playbackState.CurrentlyPlayingId);
        if (audioFeatures != null)
        {
            audioFeatures.TrackName = PlaybackState.CurrentlyPlaying ?? string.Empty;
            var audioParams = new Dictionary<string, object>
            {
                { IViewModel.ParameterAudioFeatures, audioFeatures }
            };
            _windowService.ShowWindow<AudioMetricsViewModel>(parameters: audioParams);
        }
        else
            ShowError("Error", "Failed to get audio metrics.");
    }

    [RelayCommand(CanExecute = nameof(GetAudioMetricsCanExecute))]
    private void GetAudioAnalysis()
    {
        if (_playbackState.CurrentlyPlayingId == null) return;
        _windowService.ShowWindow<AudioAnalysisViewModel>();
    }

    [RelayCommand(CanExecute = nameof(GetShareUrlCanExecute))]
    private async Task GetShareUrl()
    {
        var url = await _spotifyService.GetShareUrl(_playbackState.CurrentlyPlayingId ?? string.Empty);
        if (string.IsNullOrEmpty(url))
            ShowError("Error", "Failed to get share url.");
        else
        {
            _windowService.SetClipboardText(url);
            _toastService.ShowTextToast("info", 0, "Share URL", "Copied to clipboard");
        }
    }
    #endregion

    #region Internal Configuration
    private void SetTimers()
    {
        _progressTimer.Change(_playbackState.IsPlaying ? 0 : Timeout.Infinite, ProgressUpdateIntervalMs);
    }
    #endregion

    #region Command States

    private bool IsBusyCanExecute() => !IsBusy;

    private bool StartSongRadioCanExecute() => _playbackState.CurrentlyPlayingId != null;
    
    private bool GetAudioMetricsCanExecute() => _playbackState.IsPlaying;
    private bool GetShareUrlCanExecute() => _playbackState.IsPlaying;
    private static bool AuthorizeCanExecute() => true;
    private bool NextCanExecute() => _playbackState.IsPlaying;
    private bool PreviousCanExecute() => _playbackState.IsPlaying;
    private bool ToggleLikedCanExecute() => _playbackState.IsPlaying;
    private void UpdateCommandStates()
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            _asyncCommandList.ForEach(x => x.NotifyCanExecuteChanged());
            _commandList.ForEach(x => x.NotifyCanExecuteChanged());
        });
    }
    #endregion

    #region UI Helpers
    internal void ShowError(string title, string message)
    {
        _toastService.ShowTextToast("error", 0, title, message);
    }
    private void ShowStatus(string title, string message)
    {
        _toastService.ShowTextToast("status", 0, title, message);
    }

    [RelayCommand]
    private void OpenSettings() => _windowService.ShowWindow<ClientIdViewModel>();

    #endregion

    #region Fields
    private readonly ISpotifyService _spotifyService;
    private readonly IToastService _toastService;
    private readonly IWindowService _windowService;
    private readonly IResourceService _resourceService;
    private readonly List<IAsyncRelayCommand> _asyncCommandList;
    private readonly List<IRelayCommand> _commandList;
    private readonly Timer _progressTimer;
    private PlaybackState _playbackState = new();
    private bool _isSeeking;
    private bool _topmost;
    private const int ProgressUpdateIntervalMs = 1000;
    private User? _user;
    #endregion
}
