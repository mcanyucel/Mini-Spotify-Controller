using System.Collections.Generic;
using System.Text.Json.Serialization;
using MiniSpotifyController.model;
using MiniSpotifyController.model.Spotify;

namespace MiniSpotifyController.service.Spotify;

public class DeviceListResponse
{
    [JsonPropertyName("devices")] public IEnumerable<Device>? Devices { get; set; } = [];
}