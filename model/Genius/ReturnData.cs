using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record ReturnData(
    [property: JsonPropertyName("response")]
    Response Response
    );
