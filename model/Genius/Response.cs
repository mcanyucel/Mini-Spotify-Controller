using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record Response(
    [property: JsonPropertyName("hits")]
    IEnumerable<Hit> Hits
    );
