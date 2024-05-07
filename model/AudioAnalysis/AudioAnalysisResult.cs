using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

internal sealed record AudioAnalysisResult(
    [property: JsonPropertyName("meta")]
    Meta Meta,
    [property: JsonPropertyName("track")]
    Track Track,
    [property: JsonPropertyName("bars")]
    IEnumerable<Bar> Bars,
    [property: JsonPropertyName("beats")]
    IEnumerable<Beat> Beats,
    [property: JsonPropertyName("sections")]
    IEnumerable<Section> Sections,
    [property: JsonPropertyName("segments")]
    IEnumerable<Segment> Segments,
    [property: JsonPropertyName("tatums")]
    IEnumerable<Tatum> Tatums
    );