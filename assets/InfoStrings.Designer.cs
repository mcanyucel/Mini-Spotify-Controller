﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MiniSpotifyController.assets {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class InfoStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InfoStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MiniSpotifyController.assets.InfoStrings", typeof(InfoStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of channels used for analysis. If 1, all channels are summed together to mono before analysis.
        /// </summary>
        internal static string AnalysisChannels {
            get {
                return ResourceManager.GetString("AnalysisChannels", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The sample rate used to decide and analyze this track. It may differ from the actual sample rate of this track available on Spotify.
        /// </summary>
        internal static string AnalysisSampleRate {
            get {
                return ResourceManager.GetString("AnalysisSampleRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The amount of time taken to analyze this track in seconds.
        /// </summary>
        internal static string AnalysisTime {
            get {
                return ResourceManager.GetString("AnalysisTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The version of the Analyzer used to analyze this track.
        /// </summary>
        internal static string AnalyzerVersion {
            get {
                return ResourceManager.GetString("AnalyzerVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Musical Fingerprint (ENMFP) codestring for this track..
        /// </summary>
        internal static string Codestring {
            get {
                return ResourceManager.GetString("Codestring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Musical Fingerprint (ENMFP) code version for this track..
        /// </summary>
        internal static string CodeVersion {
            get {
                return ResourceManager.GetString("CodeVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the interval.
        /// </summary>
        internal static string ConfidenceInterval {
            get {
                return ResourceManager.GetString("ConfidenceInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the segment.
        /// </summary>
        internal static string ConfidenceSegment {
            get {
                return ResourceManager.GetString("ConfidenceSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the tatum.
        /// </summary>
        internal static string ConfidenceTatums {
            get {
                return ResourceManager.GetString("ConfidenceTatums", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A detailed status code for this track. If analysis data is missing, this code may explain why.
        /// </summary>
        internal static string Detailed_Status {
            get {
                return ResourceManager.GetString("Detailed Status", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The duration (in seconds) of the time interval.
        /// </summary>
        internal static string DuratioInterval {
            get {
                return ResourceManager.GetString("DuratioInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The duration of the track in seconds.
        /// </summary>
        internal static string Duration {
            get {
                return ResourceManager.GetString("Duration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The duration (in seconds) of the segment.
        /// </summary>
        internal static string DurationSegment {
            get {
                return ResourceManager.GetString("DurationSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The duration (in seconds) of the tatum.
        /// </summary>
        internal static string DurationTatums {
            get {
                return ResourceManager.GetString("DurationTatums", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Echoprint code for this track..
        /// </summary>
        internal static string Echoprintstring {
            get {
                return ResourceManager.GetString("Echoprintstring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Echoprint code version for this track..
        /// </summary>
        internal static string EchoprintVersion {
            get {
                return ResourceManager.GetString("EchoprintVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The time, in seconds, at which the track&apos;s fade-in period ends. If the track has no fade-in, this will be 0.0.
        /// </summary>
        internal static string EndOfFadeIn {
            get {
                return ResourceManager.GetString("EndOfFadeIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The method used to read the track&apos;s audio data.
        /// </summary>
        internal static string InputProcess {
            get {
                return ResourceManager.GetString("InputProcess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The estimated overall key of the track. The key identifies the tonic triad, the chord, major or minor, which represents the final point of rest or resolution for a piece, or the focal point of a section. Keys are given in the standard Pitch Class notation (E.g. C, C#, D, etc).
        /// </summary>
        internal static string Key {
            get {
                return ResourceManager.GetString("Key", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall confidence of the key, from 0.0 to 1.0. The confidence is higher for more popular keys, E.g. C, G, D minor etc.
        /// </summary>
        internal static string KeyConfidence {
            get {
                return ResourceManager.GetString("KeyConfidence", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the key. The confidence is higher for more popular keys, E.g. C, G, D minor etc. Songs with many key changes may correspond to low values in this field..
        /// </summary>
        internal static string KeyConfidenceInterval {
            get {
                return ResourceManager.GetString("KeyConfidenceInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The estimated overall key of the section. The key identifies the tonic triad, the chord, major or minor, which represents the final point of rest or resolution for a piece, or the focal point of a section. Keys are given in the standard Pitch Class notation (E.g. C, C#, D, etc).
        /// </summary>
        internal static string KeyInterval {
            get {
                return ResourceManager.GetString("KeyInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The overall loudness of a track in decibels (dB). Loudness values are averaged across the entire track and are useful for comparing relative loudness of tracks. Loudness is the quality of a sound that is the primary psychological correlate of physical strength (amplitude). Values typically range between -60 and 0 db.
        /// </summary>
        internal static string Loudness {
            get {
                return ResourceManager.GetString("Loudness", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The offset loudness of the segment in decibels (dB). This value should be equivalent to the loudness_start of the following segment..
        /// </summary>
        internal static string LoudnessEndSegment {
            get {
                return ResourceManager.GetString("LoudnessEndSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The overall loudness of the section in decibels (dB). Loudness values are useful for comparing relative loudness of sections within tracks..
        /// </summary>
        internal static string LoudnessInterval {
            get {
                return ResourceManager.GetString("LoudnessInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The peak loudness of the segment in decibels (dB). Combined with loudness_start and loudness_max_time, these components can be used to describe the &quot;attack&quot; of the segment..
        /// </summary>
        internal static string LoudnessMaxSegment {
            get {
                return ResourceManager.GetString("LoudnessMaxSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The moment in the segment of the peak loudness, in seconds..
        /// </summary>
        internal static string LoudnessMaxTimeSegment {
            get {
                return ResourceManager.GetString("LoudnessMaxTimeSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The onset loudness of the segment in decibels (dB). Combined with loudness_max and loudness_max_time, these components can be used to describe the &quot;attack&quot; of the segment..
        /// </summary>
        internal static string LoudnessStartSegment {
            get {
                return ResourceManager.GetString("LoudnessStartSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Modality indicates the modality of a track, the type of scale from which its melodic content is derived. This field will contain a value of 0 for &quot;minor&quot;, 1 for &quot;major&quot;, or -1 for no result. Note that the major key (e.g. C major) could more likely be confused with the minor key at 3 semitones lower (e.g. A minor).
        /// </summary>
        internal static string Mode {
            get {
                return ResourceManager.GetString("Mode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall confidence of the mode, from 0.0 to 1.0. The confidence is higher for more popular keys, E.g. C, G, D minor etc.
        /// </summary>
        internal static string ModeConfidence {
            get {
                return ResourceManager.GetString("ModeConfidence", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the mode. The confidence is higher for more popular keys, E.g. C, G, D minor etc. Songs with many key changes may correspond to low values in this field..
        /// </summary>
        internal static string ModeConfidenceInterval {
            get {
                return ResourceManager.GetString("ModeConfidenceInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Modality indicates the modality of a track, the type of scale from which its melodic content is derived. This field will contain a value of 0 for &quot;minor&quot;, 1 for &quot;major&quot;, or -1 for no result. Note that the major key (e.g. C major) could more likely be confused with the minor key at 3 semitones lower (e.g. A minor).
        /// </summary>
        internal static string ModeInterval {
            get {
                return ResourceManager.GetString("ModeInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The exact number of samples analyzed from this track. See also Analysis Sample Rate.
        /// </summary>
        internal static string NumSamples {
            get {
                return ResourceManager.GetString("NumSamples", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The offset to the start of the region of the track that was analyzed. As the entire track is analyzed, this should always be 0.
        /// </summary>
        internal static string OffsetSeconds {
            get {
                return ResourceManager.GetString("OffsetSeconds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pitch content is given by a “chroma” vector, corresponding to the 12 pitch classes C, C#, D to B, with values ranging from 0 to 1 that describe the relative dominance of every pitch in the chromatic scale. For example a C Major chord would likely be represented by large values of C, E and G (i.e. classes 0, 4, and 7). Vectors are normalized to 1 by their strongest dimension, therefore noisy sounds are likely represented by values that are all close to 1, while pure tones are described by one value at 1 (the [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PitchesSegment {
            get {
                return ResourceManager.GetString("PitchesSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The platform used to read teh track&apos;s audio data.
        /// </summary>
        internal static string Platform {
            get {
                return ResourceManager.GetString("Platform", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Rhythm code for this track..
        /// </summary>
        internal static string Rhythmstring {
            get {
                return ResourceManager.GetString("Rhythmstring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Rhythm code version for this track..
        /// </summary>
        internal static string RhythmVersion {
            get {
                return ResourceManager.GetString("RhythmVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This field willl always contain an empty string (because Spotify said so).
        /// </summary>
        internal static string SampleMD5 {
            get {
                return ResourceManager.GetString("SampleMD5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The sarting point (in seconds) of the time interval.
        /// </summary>
        internal static string StartInterval {
            get {
                return ResourceManager.GetString("StartInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The time, in seconds, at which the track&apos;s fade-out period starts. If the track has no fade-out, this will be the duration of the track.
        /// </summary>
        internal static string StartOfFadeOut {
            get {
                return ResourceManager.GetString("StartOfFadeOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The starting point (in seconds) of the segment.
        /// </summary>
        internal static string StartSegment {
            get {
                return ResourceManager.GetString("StartSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The starting point (in seconds) of the tatum.
        /// </summary>
        internal static string StartTatums {
            get {
                return ResourceManager.GetString("StartTatums", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The return code of the analyzer process. 0 if successful, 1 if any errors occurred.
        /// </summary>
        internal static string StatusCode {
            get {
                return ResourceManager.GetString("StatusCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Synch code for this track..
        /// </summary>
        internal static string Synchstring {
            get {
                return ResourceManager.GetString("Synchstring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An Echo Nest Synch code version for this track..
        /// </summary>
        internal static string SynchVersion {
            get {
                return ResourceManager.GetString("SynchVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A tatum represents the lowest regular pulse train that a listener intuitively infers from the timing of perceived musical events (segments)..
        /// </summary>
        internal static string Tatum {
            get {
                return ResourceManager.GetString("Tatum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The estimated overall tempo of a track in beats per minute (BPM). In musical terminology, tempo is the speed or pace of a given piece and derives directly from the average beat duration.
        /// </summary>
        internal static string Tempo {
            get {
                return ResourceManager.GetString("Tempo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall confidence of the tempo, from 0.0 to 1.0. Some tracks contain tempo changes or sounds which don&apos;t fit the &quot;grid&quot; which would correspond to a low confidence.
        /// </summary>
        internal static string TempoConfidence {
            get {
                return ResourceManager.GetString("TempoConfidence", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the tempo. Some tracks contain tempo changes or sounds which don&apos;t contain tempo (like pure speech) which would correspond to a low value in this field..
        /// </summary>
        internal static string TempoConfidenceInterval {
            get {
                return ResourceManager.GetString("TempoConfidenceInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The estimated overall tempo of the section in beats per minute (BPM). In musical terminology, tempo is the speed or pace of a given piece and derives directly from the average beat duration.
        /// </summary>
        internal static string TempoInterval {
            get {
                return ResourceManager.GetString("TempoInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timbre is the quality of a musical note or sound that distinguishes different types of musical instruments, or voices. It is a complex notion also referred to as sound color, texture, or tone quality, and is derived from the shape of a segment’s spectro-temporal surface, independently of pitch and loudness. The timbre feature is a vector that includes 12 unbounded values roughly centered around 0. Those values are high level abstractions of the spectral surface, ordered by degree of importance. For complete [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TimbreSegment {
            get {
                return ResourceManager.GetString("TimbreSegment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall time signature of a track. The time signature (meter) is a notational convention to specify how many beats are in each bar (or measure). The time signature ranges from 3 to 7 indicating time signatures of &quot;3/4&quot; to &quot;7/4&quot;.
        /// </summary>
        internal static string TimeSignature {
            get {
                return ResourceManager.GetString("TimeSignature", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall confidence of the time signature, from 0.0 to 1.0. Some tracks contain time signature changes, which would correspond to a low confidence.
        /// </summary>
        internal static string TimeSignatureConfidence {
            get {
                return ResourceManager.GetString("TimeSignatureConfidence", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The confidence, from 0.0 to 1.0, of the reliability of the time signature. Some tracks contain time signature changes, which would correspond to a low value in this field..
        /// </summary>
        internal static string TimeSignatureConfidenceInterval {
            get {
                return ResourceManager.GetString("TimeSignatureConfidenceInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An estimated overall time signature of a track. The time signature (meter) is a notational convention to specify how many beats are in each bar (or measure). The time signature ranges from 3 to 7 indicating time signatures of &quot;3/4&quot; to &quot;7/4&quot;.
        /// </summary>
        internal static string TimeSignatureInterval {
            get {
                return ResourceManager.GetString("TimeSignatureInterval", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Unix timestamp (in seconds) at which this track was analyzed.
        /// </summary>
        internal static string Timestamp {
            get {
                return ResourceManager.GetString("Timestamp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The length of the region of the track was analyzed, if a subset of the track was analyzed. As the entire track is analyzed, this should always be 0.
        /// </summary>
        internal static string WindowSeconds {
            get {
                return ResourceManager.GetString("WindowSeconds", resourceCulture);
            }
        }
    }
}
