namespace MiniSpotifyController.model;

internal sealed record Device(string Id, bool IsActive, bool IsPrivateSession, bool IsRestricted, string Name, string Type, int VolumePercent, bool SupportsVolume);

