using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.OAuth;
using Serilog;

namespace MiniSpotifyController.service.Spotify;

public class UserManager(IHttpClientFactory httpClientFactory, OAuthAuthenticator authenticator, ILogger logger)
{
    public async Task<User?> GetUser()
    {
        try
        {
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, UserEndpoint);
            var response = await client.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonSerializer.Deserialize<User>(responseString);
            
            logger.Error("Failed to get user: {responseString}", responseString);
            return null;
        }
        
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get user");
            return null;
        }
    }
    
    private const string UserEndpoint = "https://api.spotify.com/v1/me";
}