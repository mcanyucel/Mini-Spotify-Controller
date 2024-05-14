using MiniSpotifyController.model.AudioAnalysis;
using System.Collections.Generic;
using System.Globalization;

namespace MiniSpotifyController.window.helper;

internal static class UIHelpers
{
    public static List<AudioDataDisplayItem> ToDisplayItems(this Meta meta)
    {
        List<AudioDataDisplayItem> result = [];
        result.Add(new AudioDataDisplayItem("Analyzer Version", meta.AnalyzerVersion, assets.InfoStrings.AnalyzerVersion));
        result.Add(new AudioDataDisplayItem("Platform", meta.Platform, assets.InfoStrings.Platform));
        result.Add(new AudioDataDisplayItem("Detailed Status", meta.DetailedStatus, assets.InfoStrings.Detailed_Status));
        result.Add(new AudioDataDisplayItem("Status Code", meta.StatusCode.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.StatusCode));
        result.Add(new AudioDataDisplayItem("Timestamp", meta.Timestamp.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Timestamp));
        result.Add(new AudioDataDisplayItem("Analysis Time", meta.AnalysisTime.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.AnalysisTime));
        result.Add(new AudioDataDisplayItem("Input Process", meta.InputProcess, assets.InfoStrings.InputProcess));
        return result;
    }

    public static List<AudioDataDisplayItem> ToDisplayItems(this Track track)
    {
        List<AudioDataDisplayItem> result = [];
        result.Add(new AudioDataDisplayItem("Number of Samples", track.NumSamples.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.NumSamples));
        result.Add(new AudioDataDisplayItem("Duration", track.Duration.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Duration));
        result.Add(new AudioDataDisplayItem("Sample MD5", track.SampleMD5, assets.InfoStrings.SampleMD5));
        result.Add(new AudioDataDisplayItem("Offset Seconds", track.OffsetSeconds.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.OffsetSeconds));
        result.Add(new AudioDataDisplayItem("Window Seconds", track.WindowSeconds.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.WindowSeconds));
        result.Add(new AudioDataDisplayItem("Analysis Sample Rate", track.AnalysisSampleRate.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.AnalysisSampleRate));
        result.Add(new AudioDataDisplayItem("Analysis Channels", track.AnalysisChannels.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.AnalysisChannels));
        result.Add(new AudioDataDisplayItem("End of Fade In", track.EndOfFadeIn.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.EndOfFadeIn));
        result.Add(new AudioDataDisplayItem("Start of Fade Out", track.StartOfFadeOut.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.StartOfFadeOut));
        result.Add(new AudioDataDisplayItem("Loudness", track.Loudness.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Loudness));
        result.Add(new AudioDataDisplayItem("Tempo", track.Tempo.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Tempo));
        result.Add(new AudioDataDisplayItem("Tempo Confidence", track.TempoConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TempoConfidence));
        result.Add(new AudioDataDisplayItem("Time Signature", track.TimeSignature.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TimeSignature));
        result.Add(new AudioDataDisplayItem("Time Signature Confidence", track.TimeSignatureConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TimeSignatureConfidence));
        result.Add(new AudioDataDisplayItem("Key", track.Key.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Key));
        result.Add(new AudioDataDisplayItem("Key Confidence", track.KeyConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.KeyConfidence));
        result.Add(new AudioDataDisplayItem("Mode", track.Mode.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Mode));
        result.Add(new AudioDataDisplayItem("Mode Confidence", track.ModeConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.ModeConfidence));
        result.Add(new AudioDataDisplayItem("Code String", track.Codestring, assets.InfoStrings.Codestring, true));
        result.Add(new AudioDataDisplayItem("Code Version", track.CodeVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.CodeVersion));
        result.Add(new AudioDataDisplayItem("Echoprint String", track.EchoPrintString, assets.InfoStrings.Echoprintstring, true));
        result.Add(new AudioDataDisplayItem("Echoprint Version", track.EchoPrintVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.EchoprintVersion));
        result.Add(new AudioDataDisplayItem("Synchstring", track.SynchString, assets.InfoStrings.Synchstring, true));
        result.Add(new AudioDataDisplayItem("Synch Version", track.SynchVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.SynchVersion));
        result.Add(new AudioDataDisplayItem("Rhythmstring", track.RhythmString, assets.InfoStrings.Rhythmstring, true));
        result.Add(new AudioDataDisplayItem("Rhythm Version", track.RhythmVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.RhythmVersion));
        return result;
    }
}
