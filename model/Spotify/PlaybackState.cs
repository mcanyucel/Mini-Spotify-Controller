using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniSpotifyController.model.Spotify;

public sealed partial class PlaybackState : ObservableObject
{
    public bool IsPlaying { get; private init; }
    public SpotifyTrack? Track { get; }
    
    [ObservableProperty] private int _progressMs;
    [ObservableProperty] private bool _isLiked;
    public Device? Device { get; init; }
    
    public void IncrementProgress(int delta, bool isSeeking)
    {

        if (!isSeeking)
            ProgressMs += delta;
        else
        {
#pragma warning disable MVVMTK0034
            _progressMs += delta; // Direct field access to avoid event when seeking
#pragma warning restore MVVMTK0034
        }

    }
    public void ResetProgress() => ProgressMs = 0;
    public override bool Equals(object? obj) => obj is PlaybackState other && GetHashCode() == other.GetHashCode();
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(IsPlaying);
        hash.Add(ProgressMs);
        hash.Add(Device);
        hash.Add(Track);
        return hash.ToHashCode();
    }

    private PlaybackState()
    {
    }
    
    private PlaybackState(Device device, SpotifyTrack track, bool isPlaying, int progressMs)
    {
        Device = device;
        Track = track;
        IsPlaying = isPlaying;
        ProgressMs = progressMs;
    }
    
    public static PlaybackState CreatePaused() => new()
    {
        IsPlaying = false
    };

    public static PlaybackState CreateState(Device device,
        SpotifyTrack track,
        bool isPlaying,
        int durationMs) =>
        new(device,
            track,
            isPlaying,
            durationMs);

}
