using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record Artist(
    [property: JsonPropertyName("api_path")]
    string ApiPath,
    [property: JsonPropertyName("name")]
    string Name
    );
