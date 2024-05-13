using Microsoft.Extensions.DependencyInjection;
using MiniSpotifyController.service;
using MiniSpotifyController.service.implementation;
using MiniSpotifyController.viewmodel;
using System;
using System.Windows;

namespace MiniSpotifyController;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Services = ConfigureServices();

        this.InitializeComponent();
    }

    public new static App Current => (App)Application.Current;

    public IServiceProvider Services { get; }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ISpotifyService, SpotifyService>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<IPreferenceService, PreferenceService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<IResourceService, ResourceService>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<AuthViewModel>();
        services.AddTransient<ClientIdViewModel>();
        services.AddTransient<AudioMetricsViewModel>();
        services.AddTransient<AudioAnalysisViewModel>();
        services.AddTransient<ILyricsService, GeniusService>();
        
        return services.BuildServiceProvider();
    }
}
