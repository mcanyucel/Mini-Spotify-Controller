namespace Mini_Spotify_Controller.model
{
    /// <summary>
    /// Audio feature information for a single track.
    /// See https://developer.spotify.com/documentation/web-api/reference/get-audio-features
    /// </summary>
    public record AudioFeatures
    {
        public double Acousticness { get; set; }
        public double Danceability { get; set; }
        public double Energy { get; set; }
        public double Instrumentalness { get; set; }
        public int KeyNumber { get; set; }
        public string Key
        {
            get
            {
                return KeyNumber switch
                {
                    0 => "C",
                    1 => "C♯, D♭",
                    2 => "D",
                    3 => "D♯, E♭",
                    4 => "E",
                    5 => "F",
                    6 => "F♯, G♭",
                    7 => "G",
                    8 => "G♯, A♭",
                    9 => "A",
                    10 => "A♯, B♭",
                    11 => "B",
                    _ => "Unknown"
                };
            }
        }
        public double Liveness { get; set; }
        public double Loudness { get; set; }
        public int ModeNumber { get; set; }
        public string Mode
        {
            get
            {
                return ModeNumber switch
                {
                    0 => "Minor",
                    1 => "Major",
                    _ => "Unknown"
                };
            }
        }
        public double Tempo { get; set; }
        public double Valence { get; set; }
        public int TimeSignature { get; set; }

        #region Limits
        public static double AcousticnessMin { get => 0.0; }
        public static double AcousticnessMax { get => 1.0; }
        public static double DanceabilityMin { get => 0.0; }
        public static double DanceabilityMax { get => 1.0; }
        public static double EnergyMin { get => 0.0; }
        public static double EnergyMax { get => 1.0; }
        public static double InstrumentalnessMin { get => 0.0; }
        public static double InstrumentalnessMax { get => 1.0; }
        public static double LivenessMin { get => 0.0; }
        public static double LivenessMax { get => 1.0; }
        public static double LoudnessMin { get => -60.0; }
        public static double LoudnessMax { get => 0.0; }
        public static double TempoMin { get => 0.0; }
        public static double TempoMax { get => 300.0; }
        public static double ValenceMin { get => 0.0; }
        public static double ValenceMax { get => 1.0; }
        public static double TimeSignatureMin { get => 3.0; }
        public static double TimeSignatureMax { get => 7.0; }
        #endregion
    }
}