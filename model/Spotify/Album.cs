using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

/// <summary>
/// Album data
/// </summary>
public class Album
{
    [JsonPropertyName("album_type")] public string AlbumType { get; set; } = null!;

    [JsonPropertyName("artists")] public List<Artist> Artists { get; set; } = null!;

    [JsonPropertyName("available_markets")]
    public List<string> AvailableMarkets { get; set; } = [];

    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; } = null!;

    [JsonPropertyName("href")] public string Href { get; set; } = null!;

    [JsonPropertyName("id")] public string Id { get; set; } = null!;

    [JsonPropertyName("images")] public List<SpotifyImage> Images { get; set; } = [];

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("release_date")] public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("release_date_precision")]
    public string ReleaseDatePrecision { get; set; } = string.Empty;

    [JsonPropertyName("total_tracks")]
    public int TotalTracks { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

    [JsonPropertyName("uri")] public string Uri { get; set; } = string.Empty;
}
