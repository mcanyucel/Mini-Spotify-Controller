using MiniSpotifyController.model;
using MiniSpotifyController.window;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class WindowService : IWindowService
    {
        void IWindowService.ShowClientIdWindowDialog()
        {
            _clientIdWindow = new ClientIdWindow();
            _clientIdWindow.ShowDialog();
        }
        void IWindowService.CloseClientIdWindowDialog()
        {
            _clientIdWindow?.Close();
            _clientIdWindow = null;
        }
        void IWindowService.ShowAuthorizationWindowDialog()
        {
            _authWindow = new AuthWindow();
            _authWindow.ShowDialog();
        }
        void IWindowService.CloseAuthorizationWindowDialog()
        {
            _authWindow?.Close();
            _authWindow = null;
        }

        void IWindowService.SetClipboardText(string text) => Clipboard.SetText(text);

        void IWindowService.ShowAudioFeaturesWindow(AudioFeatures audioFeatures)
        {
            if (_audioMetricsWindow == null)
            {
                _audioMetricsWindow = new AudioMetricsWindow(audioFeatures);
                _audioMetricsWindow.Show();
                _audioMetricsWindow.Closed += (_, _) => _audioMetricsWindow = null;
            }
            else
            {
                _audioMetricsWindow.UpdateData(audioFeatures);
                _audioMetricsWindow.Activate();
            }
        }

        void IWindowService.ShowAudioAnalysisWindow()
        {
            if (_audioAnalysisWindow == null)
            {
                _audioAnalysisWindow = new AudioAnalysisWindow();
                _audioAnalysisWindow.Show();
                _audioAnalysisWindow.Closed += (_, _) => _audioAnalysisWindow = null;
            }
            else
            {
                _audioAnalysisWindow.Activate();
            }

        }

        bool IWindowService.IsAudioMetricsWindowOpen() => _audioMetricsWindow != null;

        bool IWindowService.ShowUpdateWindowDialog() => MessageBox.Show("A new version of Mini Spotify Controller is available. Do you want to download it?", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        void IWindowService.ShowDevicesContextMenu(Device[] devices, Func<string, Task> transferPlayback)
        {
            ContextMenu contextMenu = new();

            foreach (var device in devices)
            {
                MenuItem menuItem = new()
                {
                    Header = device.Name,
                    Tag = device.Id,
                    IsCheckable = true,
                    IsChecked = device.IsActive,
                };
                menuItem.Click += async (sender, _) =>
                {
                    if (sender is MenuItem item)
                    {
                        await transferPlayback(item.Tag as string ?? string.Empty);
                    }
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        void IWindowService.ShowLyricsWindow()
        {
            if (_lyricsWindow == null)
            {
                _lyricsWindow = new LyricsWindow();
                _lyricsWindow.Show();
                _lyricsWindow.Closed += (_, _) => _lyricsWindow = null;
            }
            else
            {
                _lyricsWindow.Activate();
            }
        }

        #region Fields

        private AuthWindow? _authWindow;
        private ClientIdWindow? _clientIdWindow;
        private AudioMetricsWindow? _audioMetricsWindow;
        private AudioAnalysisWindow? _audioAnalysisWindow;
        private LyricsWindow? _lyricsWindow;
        #endregion
    }
}
