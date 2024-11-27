using MiniSpotifyController.window;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MiniSpotifyController.Extensions;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.viewmodel;
using Serilog;

namespace MiniSpotifyController.service.implementation
{
    internal sealed partial class WindowService(ViewModelFactory viewModelFactory, WindowFactory windowFactory, ILogger logger) : IWindowService, IDisposable
    {
        public void ShowWindow<TViewModel>(bool isModal = false, Dictionary<string, object>? parameters = null) where TViewModel : IViewModel
        {
            int windowHash;
            if (parameters == null || !parameters.TryGetValue(IViewModel.ParameterSpotifyTrackId, out var id))
            {
                windowHash = ServiceExtensions.GetHash(typeof(TViewModel).Name);
            }
            else
            {
                windowHash = ServiceExtensions.GetHash(typeof(TViewModel).Name, id.ToString());
            }
            
            if (_openWindows.TryGetValue(windowHash, out var existingWindow))
            {
                existingWindow.Activate();
            }
            else
            {
                var viewModel = viewModelFactory.Create<TViewModel>(parameters);
                var window = windowFactory.CreateWindowForViewModel<TViewModel>();
                _openWindows.Add(windowHash, window);
                SubscribeToWindowClosed(window, windowHash);
                window.DataContext = viewModel;
                if (isModal)
                {
                    window.ShowDialog();
                }
                else
                {
                    window.Show();
                }

                if (typeof(TViewModel) == typeof(MainViewModel))
                {
                    _homeWindowHash = windowHash;
                }
            }
        }
        
        public void CloseWindow<TViewModel>(string? id = null) where TViewModel : IViewModel
        {
            var windowHash = ServiceExtensions.GetHash(typeof(TViewModel).Name, id);
            if (_openWindows.TryGetValue(windowHash, out var window))
                window.Close();
            else
                logger.Warning("Window with hash {windowHash} not found.", windowHash);
        }

        public void SetClipboardText(string text) => Clipboard.SetText(text);

        public void ShowDevicesContextMenu(Device[] devices, Func<Device, Task> transferPlayback)
        {
            ContextMenu contextMenu = new();

            foreach (var device in devices)
            {
                MenuItem menuItem = new()
                {
                    Header = device.Name,
                    Tag = device,
                    IsCheckable = true,
                    IsChecked = device.IsActive,
                };
                menuItem.Click += async (sender, _) =>
                {
                    if (sender is MenuItem { Tag: Device targetDevice }) await transferPlayback(targetDevice);
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }
        
        /// <summary>
        /// Subscribe to the Closed event of a window to remove it from the openWindows dictionary when it is closed.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="windowHash"></param>
        private void SubscribeToWindowClosed(Window window, int windowHash)
        {
            window.Closed += OnWindowClosedHandler;
            return;

            void OnWindowClosedHandler(object? _, EventArgs e)
            {
                _openWindows.Remove(windowHash);
                window.Closed -= OnWindowClosedHandler;

                if (windowHash == _homeWindowHash)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        #region Fields

        private readonly Dictionary<int, Window> _openWindows = [];
        private int _homeWindowHash;
        #endregion
        
        #region IDisposable Support
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                // Dispose managed state (managed objects)

                // Close all open windows
                foreach (var window in _openWindows.Values)
                {
                    window.Close();
                }
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null
            _disposedValue = true;
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WindowService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            // GC.SuppressFinalize(this); // uncomment if 'Dispose(bool disposing)' has code to free unmanaged resources
        }
    
        #endregion
    }
}
