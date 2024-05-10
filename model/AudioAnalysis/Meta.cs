using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

internal sealed record Meta(
    [property: JsonPropertyName("analyzer_version")]
    string AnalyzerVersion,
    [property: JsonPropertyName("platform")]
    string Platform,
    [property: JsonPropertyName("detailed_status")]
    string DetailedStatus,
    [property: JsonPropertyName("status_code")]
    int StatusCode,
    [property: JsonPropertyName("timestamp")]
    long Timestamp,
    [property: JsonPropertyName("analysis_time")]
    double AnalysisTime,
    [property: JsonPropertyName("input_process")]
    string InputProcess
    );
