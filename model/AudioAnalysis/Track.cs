using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.AudioAnalysis;

// ReSharper disable once ClassNeverInstantiated.Global - Instantiated by deserializer
internal sealed record Track(
    [property: JsonPropertyName("num_samples")]
    long NumSamples,
    [property: JsonPropertyName("duration")]
    double Duration,
    [property: JsonPropertyName("sample_md5")]
    string SampleMd5,
    [property: JsonPropertyName("offset_seconds")]
    double OffsetSeconds,
    [property: JsonPropertyName("window_seconds")]
    double WindowSeconds,
    [property: JsonPropertyName("analysis_sample_rate")]
    long AnalysisSampleRate,
    [property: JsonPropertyName("analysis_channels")]
    int AnalysisChannels,
    [property: JsonPropertyName("end_of_fade_in")]
    double EndOfFadeIn,
    [property: JsonPropertyName("start_of_fade_out")]
    double StartOfFadeOut,
    [property: JsonPropertyName("loudness")]
    double Loudness,
    [property: JsonPropertyName("tempo")]
    double Tempo,
    [property: JsonPropertyName("tempo_confidence")]
    double TempoConfidence,
    [property: JsonPropertyName("time_signature")]
    int TimeSignature,
    [property: JsonPropertyName("time_signature_confidence")]
    double TimeSignatureConfidence,
    [property: JsonPropertyName("key")]
    int Key,
    [property: JsonPropertyName("key_confidence")]
    double KeyConfidence,
    [property: JsonPropertyName("mode")]
    int Mode,
    [property: JsonPropertyName("mode_confidence")]
    double ModeConfidence,
    // ReSharper disable once StringLiteralTypo
    [property: JsonPropertyName("codestring")]
    // ReSharper disable once StringLiteralTypo
    string Codestring,
    // ReSharper disable once StringLiteralTypo
    [property: JsonPropertyName("code_version")]
    double CodeVersion,
    // ReSharper disable once StringLiteralTypo
    [property: JsonPropertyName("echoprintstring")]
    string EchoPrintString,
    // ReSharper disable once StringLiteralTypo
    [property: JsonPropertyName("echoprint_version")]
    double EchoPrintVersion,
    // ReSharper disable once StringLiteralTypo
    [property: JsonPropertyName("synchstring")]
    string SynchString,
    [property: JsonPropertyName("synch_version")]
    double SynchVersion,
    [property: JsonPropertyName("rhythmstring")]
    string RhythmString,
    [property: JsonPropertyName("rhythm_version")]
    double RhythmVersion
    );
