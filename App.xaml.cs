using Microsoft.Extensions.DependencyInjection;
using Mini_Spotify_Controller.service;
using Mini_Spotify_Controller.service.implementation;
using Mini_Spotify_Controller.viewmodel;
using System;
using System.Windows;

namespace Mini_Spotify_Controller
{
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

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ISpotifyService, SpotifyService>();
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<IPreferenceService, PreferenceService>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<AuthViewModel>();
            services.AddTransient<ClientIdViewModel>();
            return services.BuildServiceProvider();
        }
    }
}
