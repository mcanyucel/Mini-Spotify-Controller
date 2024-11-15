using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

public sealed record Bar(
    [property: JsonPropertyName("start")]
    double Start,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("confidence")]
    double Confidence
    );
