using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

internal sealed record Segment(
    [property: JsonPropertyName("start")]
    double Start,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("confidence")]
    double Confidence,
    [property: JsonPropertyName("loudness_start")]
    double LoudnessStart,
    [property: JsonPropertyName("loudness_max_time")]
    double LoudnessMaxTime,
    [property: JsonPropertyName("loudness_max")]
    double LoudnessMax,
    [property: JsonPropertyName("loudness_end")]
    double LoudnessEnd,
    [property: JsonPropertyName("pitches")]
    IEnumerable<double> Pitches,
    [property: JsonPropertyName("timbre")]
    IEnumerable<double> Timbre
    );
