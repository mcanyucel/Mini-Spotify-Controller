using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MiniSpotifyController.service.implementation
{
    internal class GeniusService(IPreferenceService preferenceService) : ILyricsService, IDisposable
    {
        public async Task<string> GetLyrics(string songName, string artist)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Genius API credentials are not set.");
            }

            // replace spaces with %20 in the song name
            var searchQuery = songName.Replace(" ", "%20");
            var searchUrl = $"{SEARCH_ENDPOINT}{searchQuery}";
            HttpRequestMessage httpRequest = new(HttpMethod.Get, searchUrl);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Headers.Add("User-Agent", clientId);
            var response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;

        }

        public void Dispose() => httpClient.Dispose();

        const string SEARCH_ENDPOINT = "https://api.genius.com/search?q=";

        readonly HttpClient httpClient = new();
        readonly IPreferenceService preferenceService = preferenceService;
        string? clientId = preferenceService.GetGeniusClientId();
        string? accessToken = preferenceService.GetGeniusAccessToken();
    }
}
