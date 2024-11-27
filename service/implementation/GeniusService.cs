using HtmlAgilityPack;
using MiniSpotifyController.model.Genius;
using MiniSpotifyController.model.Lyrics;
using SimMetrics.Net.Metric;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniSpotifyController.service.implementation;

internal sealed partial class GeniusService(IPreferenceService preferenceService) : ILyricsService, IDisposable
{
    public async Task<LyricsResult> GetLyrics(string songName, string artist)
    {
        LyricsResult result;

        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_accessToken))
            LyricsResult.CreateError("Genius API credentials not found");

        // replace spaces with %20 in the song name
        var searchQuery = songName.Replace(" ", "%20");
        var searchUrl = $"{SearchEndpoint}{searchQuery}";
        HttpRequestMessage httpRequest = new(HttpMethod.Get, searchUrl);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        httpRequest.Headers.Add("User-Agent", _clientId);
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseRoot = await Task.Run(() => JsonSerializer.Deserialize<ReturnData>(responseContent, _jsonOptions));

        if (responseRoot is null)
            result = LyricsResult.CreateError("Failed to deserialize Genius API response");
        else
        {
            var hits = responseRoot.Response.Hits;
            var hitsList = hits.ToList();
            if (hitsList.Count != 0)
            {
                Hit finalMatch;
                LyricsResultType resultType;

                var nameMatches = hitsList.Where(hit => hit.Result.Title.Equals(songName, StringComparison.OrdinalIgnoreCase));
                var nameMatchesList = nameMatches.ToList();
                if (nameMatchesList.Count != 0)
                {
                    var artistMatches = nameMatchesList.Where(hit => hit.Result.PrimaryArtist.Name.Equals(artist, StringComparison.OrdinalIgnoreCase));

                    var artistMatchesList = artistMatches.ToList();
                    if (artistMatchesList.Count != 0)
                    {
                        // assume the first match is the correct one
                        finalMatch = artistMatchesList.First();
                        resultType = LyricsResultType.ExactMatch;
                    }
                    else
                    {
                        // name matches but artist does not - possibly a cover. Return the match with the highest page views
                        finalMatch = nameMatchesList.OrderByDescending(hit => hit.Result.Stats.Pageviews).First();
                        resultType = LyricsResultType.NameMatch;
                    }

                }
                else
                {
                    // name does not match - return the match whose name is closest to the search query name
                    SmithWaterman smithWaterman = new();
                    var bestMatch = hitsList.Select(hit => new { Hit = hit, Distance = smithWaterman.GetSimilarity(songName, hit.Result.Title) })
                                        .OrderByDescending(match => match.Distance)
                                        .First();

                    finalMatch = bestMatch.Hit;
                    resultType = LyricsResultType.SimilarMatch;
                }

                var lyricsUrl = finalMatch.Result.Url;
                var lyrics = await GetLyricsFromUrl(lyricsUrl);
                result = LyricsResult.CreateMatch(resultType, finalMatch.Result.Title, finalMatch.Result.PrimaryArtist.Name, lyrics, lyricsUrl);
            }
            else
                result = LyricsResult.CreateNoResult();
        }
        return result;
    }

    private async Task<string> GetLyricsFromUrl(string url)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        // lyrics are in a div where data-lyrics-container attribute is set to "true"
        HtmlDocument doc = new();
        doc.LoadHtml(responseContent);
        var lyricsNode = doc.DocumentNode.SelectSingleNode("//div[@data-lyrics-container='true']");
        var innerHtml = lyricsNode.InnerHtml;
        // handle breaks
        innerHtml = innerHtml.Replace("<br>", "\n");
        // decode HTML entities
        responseContent = System.Net.WebUtility.HtmlDecode(innerHtml);
        // remove all HTML tags
        responseContent = HtmlRegex().Replace(responseContent, string.Empty);
        return responseContent;
    }

    public void Dispose() => _httpClient.Dispose();

    private const string SearchEndpoint = "https://api.genius.com/search?q=";

    private readonly HttpClient _httpClient = new();
    private readonly string? _clientId = preferenceService.GetGeniusClientId();
    private readonly string? _accessToken = preferenceService.GetGeniusAccessToken();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
    };

    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex HtmlRegex();
}
