using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class ExternalUrls
{
    [JsonPropertyName("spotify")]
    public string Spotify { get; set; } = string.Empty;
}