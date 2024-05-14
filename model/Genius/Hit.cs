using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record Hit(
    [property: JsonPropertyName("type")]
    string Type,
    [property: JsonPropertyName("result")]
    Result Result);
