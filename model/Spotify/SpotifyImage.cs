using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class SpotifyImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}