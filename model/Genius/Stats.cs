using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record Stats(
    [property: JsonPropertyName("pageviews")]
    long Pageviews
    );
