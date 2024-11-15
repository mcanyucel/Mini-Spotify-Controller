using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class ExternalIds
{
    [JsonPropertyName("isrc")]
    public string Isrc { get; set; } = string.Empty;
}