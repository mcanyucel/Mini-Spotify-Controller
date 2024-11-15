using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.model.AudioFeature;
using MiniSpotifyController.service;
using Serilog;

namespace MiniSpotifyController.viewmodel;

internal sealed partial class AudioMetricsViewModel : ObservableObject, IViewModel
{
    [ObservableProperty] private AudioFeatures? _audioFeatures;
    [ObservableProperty] private bool _isBusy;
    private readonly string _trackId;
    
    [ObservableProperty] private string? _trackName;
    private readonly ISpotifyService _spotifyService;
    private readonly IToastService _toastService;
    private readonly ILogger _logger;
    

    public AudioMetricsViewModel(Dictionary<string, object> parameters, ISpotifyService spotifyService, ILogger logger, IToastService toastService)
    {
        var trackId = parameters[IViewModel.ParameterSpotifyTrackId] as string;
        if (string.IsNullOrEmpty(trackId))
            throw new ArgumentNullException(nameof(parameters), @"TrackId is required");
        
        TrackName = parameters[IViewModel.ParameterTrackName] as string ?? "Unknown Track";
        
        _trackId = trackId;
        _spotifyService = spotifyService;
        _logger = logger;
        _toastService = toastService;
    }
    
    [RelayCommand]
    private async Task LoadData()
    {
        IsBusy = true;
        try
        {
            AudioFeatures = await _spotifyService.GetAudioFeatures(_trackId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load audio features for track {trackId}", _trackId);
            _toastService.ShowTextToast("error",1,"Error", "Failed to load audio features");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
