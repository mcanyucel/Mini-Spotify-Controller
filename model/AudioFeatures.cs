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
        public const double AcousticnessMin = 0.0;
        public const double AcousticnessMax = 1.0;
        public const double DanceabilityMin = 0.0;
        public const double DanceabilityMax = 1.0;
        public const double EnergyMin = 0.0;
        public const double EnergyMax = 1.0;
        public const double InstrumentalnessMin = 0.0;
        public const double InstrumentalnessMax = 1.0;
        public const double LivenessMin = 0.0;
        public const double LivenessMax = 1.0;
        public const double LoudnessMin = -60.0;
        public const double LoudnessMax = 0.0;
        public const double TempoMin = 0;
        public const double TempoMax = 250;
        public const double ValenceMin = 0.0;
        public const double ValenceMax = 1.0;
        public const double TimeSignatureMin = 3.0;
        public const double TimeSignatureMax = 7.0;
        #endregion
    }
}