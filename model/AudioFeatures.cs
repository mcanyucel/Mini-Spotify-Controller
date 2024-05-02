using System.Collections.Generic;

namespace MiniSpotifyController.model;

/// <summary>
/// Audio feature information for a single track.
/// See https://developer.spotify.com/documentation/web-api/reference/get-audio-features
/// </summary>
public record AudioFeatures
{
    public string TrackId { get; private set; }
    public string TrackName { get; set; } = string.Empty;
    public List<AudioFeature> Features { get; } = [];

    public AudioFeatures(string trackId) => TrackId = trackId;
}