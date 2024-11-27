using System.Text.Json.Serialization;

namespace MiniSpotifyController.model;

internal sealed class AccessData
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
