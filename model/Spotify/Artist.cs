using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class Artist
{
    [JsonPropertyName("external_urls")] public ExternalUrls ExternalUrls { get; set; } = null!;

    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty; 

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}