using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class SpotifyTrack
{
    [JsonPropertyName("album")] public Album Album { get; init; } = null!;

    [JsonPropertyName("artists")] public List<Artist> Artists { get; init; } = [];

    [JsonPropertyName("available_markets")]
    public List<string> AvailableMarkets { get; init; } = [];

    [JsonPropertyName("disc_number")]
    public int DiscNumber { get; init; }

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; init; }

    [JsonPropertyName("explicit")]
    public bool Explicit { get; init; }

    [JsonPropertyName("external_ids")] public ExternalIds ExternalIds { get; init; } = null!;

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; init; } = null!;

    [JsonPropertyName("href")] public string Href { get; init; } = null!;

    [JsonPropertyName("id")] public string Id { get; init; } = null!;

    [JsonPropertyName("is_local")]
    public bool IsLocal { get; init; }

    [JsonPropertyName("name")] public string Name { get; init; } = null!;

    [JsonPropertyName("popularity")]
    public int Popularity { get; init; }

    [JsonPropertyName("preview_url")] public string PreviewUrl { get; init; } = string.Empty;

    [JsonPropertyName("track_number")]
    public int TrackNumber { get; init; }

    [JsonPropertyName("type")] public string Type { get; init; } = null!;

    [JsonPropertyName("uri")] public string Uri { get; init; } = null!;

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Album);
        hash.Add(Artists);
        hash.Add(DiscNumber);
        hash.Add(DurationMs);
        hash.Add(Explicit);
        hash.Add(ExternalIds);
        hash.Add(ExternalUrls);
        hash.Add(Href);
        hash.Add(Id);
        hash.Add(IsLocal);
        hash.Add(Name);
        hash.Add(Popularity);
        hash.Add(PreviewUrl);
        hash.Add(TrackNumber);
        hash.Add(Type);
        hash.Add(Uri);
        return hash.ToHashCode();
    }
}

