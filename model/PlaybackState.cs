using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniSpotifyController.model;

public sealed partial class PlaybackState : ObservableObject
{

    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private string? _currentlyPlayingId;
    [ObservableProperty] private string? _currentlyPlayingSpotifyId;
    [ObservableProperty] private string? _currentlyPlaying;
    [ObservableProperty] private string? _currentlyPlayingArtist;
    [ObservableProperty] private Album? _currentlyPlayingAlbum;
    [ObservableProperty] private string? _deviceId;
    [ObservableProperty] private int _progressMs;
    [ObservableProperty] private int _durationMs;
    [ObservableProperty] private bool _isLiked;
    [ObservableProperty] private bool _isBusy;
    public void IncrementProgress(int delta, bool isSeeking)
    {

        if (!isSeeking)
            ProgressMs += delta;
        else
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
            _progressMs += delta;
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }
    public void ResetProgress() => ProgressMs = 0;
    public void SetProgress(int progress) => ProgressMs = progress;
    public override bool Equals(object? obj) => obj is PlaybackState other && GetHashCode() == other.GetHashCode();
    public override int GetHashCode()
    {
        System.HashCode hash = new();
        hash.Add(IsPlaying);
        hash.Add(CurrentlyPlayingId);
        hash.Add(CurrentlyPlayingSpotifyId);
        hash.Add(CurrentlyPlaying);
        hash.Add(CurrentlyPlayingArtist);
        hash.Add(CurrentlyPlayingAlbum);
        hash.Add(DeviceId);
        hash.Add(ProgressMs);
        hash.Add(DurationMs);
        hash.Add(IsLiked);
        hash.Add(IsBusy);
        return hash.ToHashCode();
    }
}
