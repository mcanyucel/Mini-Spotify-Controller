using System;
using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Spotify;

public class Device
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }
    [JsonPropertyName("is_private_session")]
    public bool IsPrivateSession { get; init; }
    [JsonPropertyName("is_restricted")]
    public bool IsRestricted { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    [JsonPropertyName("volume_percent")]
    public int VolumePercent { get; init; }
    [JsonPropertyName("supports_volume")]
    public bool SupportsVolume { get; init; }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        hash.Add(IsActive);
        hash.Add(IsPrivateSession);
        hash.Add(IsRestricted);
        hash.Add(Name);
        hash.Add(Type);
        hash.Add(VolumePercent);
        hash.Add(SupportsVolume);
        return hash.ToHashCode();
    }
}

