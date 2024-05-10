﻿using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

internal sealed record Beat(
    [property: JsonPropertyName("start")]
    double Start,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("confidence")]
    double Confidence
    );
