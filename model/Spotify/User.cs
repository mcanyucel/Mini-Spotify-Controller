using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public sealed record User(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("country")] string Country
);
