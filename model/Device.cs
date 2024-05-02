namespace Mini_Spotify_Controller.model;

internal record Device(string Id,  bool IsActive, bool IsPrivateSession, bool IsRestricted, string Name, string Type, int VolumePercent, bool SupportsVolume);

