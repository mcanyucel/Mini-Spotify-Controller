using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.model.AudioFeature;
using MiniSpotifyController.OAuth;
using Serilog;

namespace MiniSpotifyController.service.Spotify;

public class AudioManager(IHttpClientFactory httpClientFactory, OAuthAuthenticator authenticator, ILogger logger)
{
    public async Task<AudioFeatures?> GetAudioFeatures(string spotifyId)
    {
        try
        {
            var endPoint = $"{AudioFeaturesEndpoint}/{spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, endPoint);
            
            var response = await client.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.Error("Failed to get audio features for track {spotifyId}: {responseString}", spotifyId, responseString);
                return null;
            }
            
            return GenerateAudioFeatures(responseString);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get audio features for track {spotifyId}", spotifyId);
            return null;
        }
    }

    public async Task<AudioAnalysisResult?> GetAudioAnalysis(string spotifyId)
    {
        try
        {
            var endPoint = $"{AudioAnalysisEndpoint}/{spotifyId}";
            var client = httpClientFactory.CreateClient(App.SpotifyWebApiClientName);
            var httpRequestMessage = await authenticator.CreateAuthorizedHttpRequestMessage(HttpMethod.Get, endPoint);
            
            var response = await client.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode) return JsonSerializer.Deserialize<AudioAnalysisResult>(responseString);
            
            logger.Error("Failed to get audio analysis for track {spotifyId}: {responseString}", spotifyId, responseString);
            return null;
        }
        
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to get audio analysis for track {spotifyId}", spotifyId);
            return null;
        }
    }

    private static AudioFeatures GenerateAudioFeatures(string featuresResponseContent)
    {
        var jsonDocumentRoot = JsonDocument.Parse(featuresResponseContent).RootElement;
        var features = new AudioFeatures(jsonDocumentRoot.GetProperty("id").GetString() ?? string.Empty);
        var featureList = new List<AudioFeature>();
        
        var featureProperties = jsonDocumentRoot.EnumerateObject();
        foreach (var featureProperty in featureProperties)
        {
            var template = AudioFeatureTemplates.FirstOrDefault(template => template.FeatureName == featureProperty.Name);
            if (template == null) continue;
            
            var value = featureProperty.Value.GetDouble();
            if (double.IsNaN(value)) continue;
            featureList.Add(template.CloneWithNewValue(value));
        }
        
        features.Features.AddRange(featureList);
        return features;
    }

    private const string AudioFeaturesEndpoint = "https://api.spotify.com/v1/audio-features";
    private const string AudioAnalysisEndpoint = "https://api.spotify.com/v1/audio-analysis";

    private static readonly List<AudioFeature> AudioFeatureTemplates =
    [
        new("danceability", double.NaN, 0d, 1d),
        new("energy", double.NaN, 0d, 1d),
        new("key", double.NaN, 0, 11, FeatureType.Text),
        new("loudness", double.NaN, -60, 0),
        new("mode", double.NaN, 0, 1, FeatureType.Text),
        new("speechiness", double.NaN, 0d, 1d),
        new("acousticness", double.NaN, 0d, 1d),
        new("instrumentalness", double.NaN, 0d, 1d),
        new("liveness", double.NaN, 0d, 1d),
        new("valence", double.NaN, 0d, 1d),
        new("tempo", double.NaN, 0d, 200d)
    ];

}