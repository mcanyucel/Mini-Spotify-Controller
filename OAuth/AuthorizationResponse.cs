using System.Text.Json.Serialization;

namespace MiniSpotifyController.OAuth;

public class AuthorizationResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; } // Bearer
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}