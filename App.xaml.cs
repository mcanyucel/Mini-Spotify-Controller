using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using MiniSpotifyController.Extensions;
using MiniSpotifyController.service;
using MiniSpotifyController.service.implementation;
using MiniSpotifyController.viewmodel;
using MiniSpotifyController.window;
using Serilog;

namespace MiniSpotifyController;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App()
    {
        ConfigureLogger();
        Services = ConfigureServices();
        InitializeComponent();
    }

    public new static App Current => (App)Application.Current;
    
    public const string SpotifyWebApiClientName = "SpotifyWebApiClient";
    public const string SpotifyAuthClientName = "SpotifyAuthClient";

    private IServiceProvider Services { get; }

    private static void ConfigureLogger()
    {
        var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniSpotifyController", "logs");

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day, formatProvider:CultureInfo.InvariantCulture);
        
#if DEBUG
        loggerConfiguration.MinimumLevel.Debug();
#else
        loggerConfiguration.MinimumLevel.Error();
#endif
        
        Log.Logger = loggerConfiguration.CreateLogger();
        Log.Information("Application started");
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddLogger()
            .AddHttpClientFactory()
            .AddOAuthAuthenticator()
            .AddCoreServices()
            .AddViewModelMapping()
            .AddViewModelFactory()
            .AddWindowFactory()
            .BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var windowService = Services.GetRequiredService<IWindowService>();
        windowService.ShowWindow<MainViewModel>();
    }
}
