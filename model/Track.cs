namespace Mini_Spotify_Controller.model
{
    internal record TrackData
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Artist { get; private set; }
        public string? Href { get; set; }
        public AudioFeatures? AudioFeatures { get; set; }
        public AudioAnalysis? AudioAnalysis { get; set; }

        public TrackData(string id, string name, string artist)
        {
            Id = id;
            Name = name;
            Artist = artist;
        }
    }
}
