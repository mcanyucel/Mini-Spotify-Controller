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

internal partial class GeniusService(IPreferenceService preferenceService) : ILyricsService, IDisposable
{
    public async Task<LyricsResult> GetLyrics(string songName, string artist)
    {
        LyricsResult result;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(accessToken))
            result = LyricsResult.CreateError("Genius API credentials not found");

        // replace spaces with %20 in the song name
        var searchQuery = songName.Replace(" ", "%20");
        var searchUrl = $"{SEARCH_ENDPOINT}{searchQuery}";
        HttpRequestMessage httpRequest = new(HttpMethod.Get, searchUrl);
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Headers.Add("User-Agent", clientId);
        var response = await httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        ReturnData? responseRoot = await Task.Run(()=>JsonSerializer.Deserialize<ReturnData>(responseContent, jsonOptions));

        if (responseRoot is null)
            result = LyricsResult.CreateError("Failed to deserialize Genius API response");
        else
        {
            var hits = responseRoot.Response.Hits;
            if (hits.Any())
            {
                Hit finalMatch;
                LyricsResultType resultType;

                var nameMatches = hits.Where(hit => hit.Result.Title.Equals(songName, StringComparison.OrdinalIgnoreCase));
                if (nameMatches.Any())
                {
                    var artistMatches = nameMatches.Where(hit => hit.Result.PrimaryArtist.Name.Equals(artist, StringComparison.OrdinalIgnoreCase));

                    if (artistMatches.Any())
                    {
                        // assume the first match is the correct one
                        finalMatch = artistMatches.First();
                        resultType = LyricsResultType.ExactMatch;
                    }
                    else
                    {
                        // name matches but artist does not - possibly a cover. Return the match with the highest page views
                        finalMatch = nameMatches.OrderByDescending(hit => hit.Result.Stats.Pageviews).First();
                        resultType = LyricsResultType.NameMatch;
                    }
                    
                }
                else
                {
                    // name does not match - return the match whose name is closest to the search query name
                    SmithWaterman smithWaterman = new();
                    var bestMatch = hits.Select(hit => new { Hit = hit, Distance = smithWaterman.GetSimilarity(songName, hit.Result.Title) })
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

    async Task<string> GetLyricsFromUrl(string url)
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
        var response = await httpClient.SendAsync(httpRequest);
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

    public void Dispose() => httpClient.Dispose();

    const string SEARCH_ENDPOINT = "https://api.genius.com/search?q=";

    readonly HttpClient httpClient = new();
    readonly string? clientId = preferenceService.GetGeniusClientId();
    readonly string? accessToken = preferenceService.GetGeniusAccessToken();
    readonly JsonSerializerOptions jsonOptions = new() 
    { 
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
    };

    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex HtmlRegex();
}
