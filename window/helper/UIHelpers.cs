using MiniSpotifyController.model.AudioAnalysis;
using System.Collections.Generic;
using System.Globalization;

namespace MiniSpotifyController.window.helper;

internal static class UiHelpers
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

    public static List<AudioDataDisplayItem> ToDisplayItems(this TrackAnalysisData trackAnalysisData)
    {
        List<AudioDataDisplayItem> result = [];
        result.Add(new AudioDataDisplayItem("Number of Samples", trackAnalysisData.NumSamples.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.NumSamples));
        result.Add(new AudioDataDisplayItem("Duration", trackAnalysisData.Duration.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Duration));
        result.Add(new AudioDataDisplayItem("Sample MD5", trackAnalysisData.SampleMd5, assets.InfoStrings.SampleMD5));
        result.Add(new AudioDataDisplayItem("Offset Seconds", trackAnalysisData.OffsetSeconds.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.OffsetSeconds));
        result.Add(new AudioDataDisplayItem("Window Seconds", trackAnalysisData.WindowSeconds.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.WindowSeconds));
        result.Add(new AudioDataDisplayItem("Analysis Sample Rate", trackAnalysisData.AnalysisSampleRate.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.AnalysisSampleRate));
        result.Add(new AudioDataDisplayItem("Analysis Channels", trackAnalysisData.AnalysisChannels.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.AnalysisChannels));
        result.Add(new AudioDataDisplayItem("End of Fade In", trackAnalysisData.EndOfFadeIn.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.EndOfFadeIn));
        result.Add(new AudioDataDisplayItem("Start of Fade Out", trackAnalysisData.StartOfFadeOut.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.StartOfFadeOut));
        result.Add(new AudioDataDisplayItem("Loudness", trackAnalysisData.Loudness.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Loudness));
        result.Add(new AudioDataDisplayItem("Tempo", trackAnalysisData.Tempo.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Tempo));
        result.Add(new AudioDataDisplayItem("Tempo Confidence", trackAnalysisData.TempoConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TempoConfidence));
        result.Add(new AudioDataDisplayItem("Time Signature", trackAnalysisData.TimeSignature.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TimeSignature));
        result.Add(new AudioDataDisplayItem("Time Signature Confidence", trackAnalysisData.TimeSignatureConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.TimeSignatureConfidence));
        result.Add(new AudioDataDisplayItem("Key", trackAnalysisData.Key.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Key));
        result.Add(new AudioDataDisplayItem("Key Confidence", trackAnalysisData.KeyConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.KeyConfidence));
        result.Add(new AudioDataDisplayItem("Mode", trackAnalysisData.Mode.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.Mode));
        result.Add(new AudioDataDisplayItem("Mode Confidence", trackAnalysisData.ModeConfidence.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.ModeConfidence));
        result.Add(new AudioDataDisplayItem("Code String", trackAnalysisData.Codestring, assets.InfoStrings.Codestring, true));
        result.Add(new AudioDataDisplayItem("Code Version", trackAnalysisData.CodeVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.CodeVersion));
        result.Add(new AudioDataDisplayItem("Echoprint String", trackAnalysisData.EchoPrintString, assets.InfoStrings.Echoprintstring, true));
        result.Add(new AudioDataDisplayItem("Echoprint Version", trackAnalysisData.EchoPrintVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.EchoprintVersion));
        result.Add(new AudioDataDisplayItem("Synchstring", trackAnalysisData.SynchString, assets.InfoStrings.Synchstring, true));
        result.Add(new AudioDataDisplayItem("Synch Version", trackAnalysisData.SynchVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.SynchVersion));
        result.Add(new AudioDataDisplayItem("Rhythmstring", trackAnalysisData.RhythmString, assets.InfoStrings.Rhythmstring, true));
        result.Add(new AudioDataDisplayItem("Rhythm Version", trackAnalysisData.RhythmVersion.ToString(CultureInfo.InvariantCulture), assets.InfoStrings.RhythmVersion));
        return result;
    }
}
