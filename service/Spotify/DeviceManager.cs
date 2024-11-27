using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.OAuth;

namespace MiniSpotifyController.service.Spotify;

public class DeviceManager(OAuthAuthenticator authenticator, IHttpClientFactory httpClientFactory)
{
    public async Task<Device?> GetLastListenedDevice()
    {
        var devices = (await GetDevices()).ToList();
        // if there are any active devices, return the first one
        var activeDevice = devices.FirstOrDefault(device => device.IsActive);
        // if not, return the first device, or null if there are no devices
        return activeDevice ?? devices.FirstOrDefault();
    }

    public async Task<IEnumerable<Device>> GetDevices()
    {
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
        var httpDevicesRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, DevicesEndPoint);
        var response = await client.SendAsync(httpDevicesRequestMessage);
        if (!response.IsSuccessStatusCode) return [];
        
        var responseString = await response.Content.ReadAsStringAsync();
        var deviceResponse = JsonSerializer.Deserialize<DeviceListResponse>(responseString);
        
        return deviceResponse is { Devices: not null } ? deviceResponse.Devices : [];
    }

    public async Task<bool> TransferPlayback(Device device)
    {
        bool result;
        var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);

        try
        {
            var httpTransferRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Put, TransferPlaybackEndpoint);
            httpTransferRequestMessage.Content = new StringContent(JsonSerializer.Serialize(new
                {
                    device_ids = new[]
                    {
                        device.Id
                    }
                }),
                Encoding.UTF8,
                "application/json");
            var response = await client.SendAsync(httpTransferRequestMessage);
            result = response.IsSuccessStatusCode;
        }
        catch
        {
            result = false;
        }
        return result;
    }
    
    
    private const string DevicesEndPoint = "https://api.spotify.com/v1/me/player/devices";
    private const string TransferPlaybackEndpoint = "https://api.spotify.com/v1/me/player";
}