using System.Collections.Generic;

namespace MiniSpotifyController.model.AudioFeature;

/// <summary>
/// Audio feature information for a single track.
/// See https://developer.spotify.com/documentation/web-api/reference/get-audio-features
/// </summary>
public record AudioFeatures(string TrackId)
{
    public string TrackName { get; set; } = string.Empty;
    public List<AudioFeature> Features { get; } = [];
}