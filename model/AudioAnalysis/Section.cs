using System;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

internal sealed record Section(
    [property: JsonPropertyName("start")]
    double Start,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("confidence")]
    double Confidence,
    [property: JsonPropertyName("loudness")]
    double Loudness,
    [property: JsonPropertyName("tempo")]
    double Tempo,
    [property: JsonPropertyName("tempo_confidence")]
    double TempoConfidence,
    [property: JsonPropertyName("key")]
    int Key,
    [property: JsonPropertyName("key_confidence")]
    double KeyConfidence,
    [property: JsonPropertyName("mode")]
    int Mode,
    [property: JsonPropertyName("mode_confidence")]
    double ModeConfidence,
    [property: JsonPropertyName("time_signature")]
    double TimeSignature,
    [property: JsonPropertyName("time_signature_confidence")]
    double TimeSignatureConfidence
    );
