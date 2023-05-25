namespace Mini_Spotify_Controller.model
{
    internal class Device
    {
        public string? Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrivateSession { get; set; }
        public bool IsRestricted { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int VolumePercent { get; set; }
    }
}
