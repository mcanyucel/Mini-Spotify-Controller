using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.OAuth;
using MiniSpotifyController.service;
using MiniSpotifyController.service.implementation;
using MiniSpotifyController.service.Spotify;
using MiniSpotifyController.viewmodel;
using MiniSpotifyController.window;
using Serilog;

namespace MiniSpotifyController.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IToastService, ToastService>();
        serviceCollection.AddSingleton<IPreferenceService, PreferenceService>();
        serviceCollection.AddSingleton<IWindowService, WindowService>();
        serviceCollection.AddSingleton<IResourceService, ResourceService>();
        serviceCollection.AddTransient<ILyricsService, GeniusService>();
        
        return serviceCollection;
    }

    public static IServiceCollection AddSpotifyServices(this IServiceCollection serviceCollection)
    {
        AddOAuthAuthenticator(serviceCollection);
        serviceCollection.AddSingleton<DeviceManager>();
        serviceCollection.AddSingleton<TrackManager>();
        serviceCollection.AddSingleton<PlaybackManager>();
        serviceCollection.AddSingleton<AudioManager>();
        serviceCollection.AddSingleton<RecommendationManager>();
        serviceCollection.AddSingleton<UserManager>();
        serviceCollection.AddSingleton<ISpotifyService, SpotifyService>();
        
        
        return serviceCollection;
    }

    public static IServiceCollection AddViewModelMapping(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDictionary<Type, Type>>(_ => new Dictionary<Type, Type>
        {
            { typeof(AudioAnalysisViewModel), typeof(AudioAnalysisWindow)},
            { typeof(AudioMetricsViewModel), typeof(AudioMetricsWindow)},
            { typeof(ClientIdViewModel), typeof(ClientIdWindow)},
            { typeof(LyricsViewModel), typeof(LyricsWindow)},
            { typeof(MainViewModel), typeof(MainWindow)}
        });
        
        return serviceCollection;
    }

    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILogger>(_ => Log.Logger);
        return serviceCollection;
    }
    
    public static IServiceCollection AddHttpClientFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();
        return serviceCollection;
    }
    
    public static IServiceCollection AddViewModelFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ViewModelFactory>();
        return serviceCollection;
    }
    
    public static IServiceCollection AddWindowFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<WindowFactory>();
        return serviceCollection;
    }
    
    private static void AddOAuthAuthenticator(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<OAuthAuthenticator>(provider =>
        {
            var preferenceService = provider.GetRequiredService<IPreferenceService>();
            var windowService = provider.GetRequiredService<IWindowService>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var clientId = preferenceService.GetClientId();
            var config = new OAuthConfig
            {
                AuthBaseUrl = "https://accounts.spotify.com/authorize",
                ClientId = clientId,
                RedirectUrl = "http://localhost:4002",
                Scopes =
                    "user-read-private user-read-email user-library-read user-library-modify user-read-playback-state user-modify-playback-state app-remote-control streaming",
                TokenUrl = "https://accounts.spotify.com/api/token",
                RevokeUrl = "https://accounts.spotify.com/authorize" 
            };
            
            var tokenStorage = new RegistryTokenStorage(@"SOFTWARE\MiniSpotifyController", "RefreshToken");
            return new OAuthAuthenticator(config, tokenStorage, preferenceService, windowService, httpClientFactory);
            
        });
    }
    
}