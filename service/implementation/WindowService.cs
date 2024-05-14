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
            clientIdWindow = new ClientIdWindow();
            clientIdWindow.ShowDialog();
        }
        void IWindowService.CloseClientIdWindowDialog()
        {
            clientIdWindow?.Close();
            clientIdWindow = null;
        }
        void IWindowService.ShowAuthorizationWindowDialog()
        {
            authWindow = new AuthWindow();
            authWindow.ShowDialog();
        }
        void IWindowService.CloseAuthorizationWindowDialog()
        {
            authWindow?.Close();
            authWindow = null;
        }

        void IWindowService.SetClipboardText(string text) => Clipboard.SetText(text);

        void IWindowService.ShowAudioFeaturesWindow(AudioFeatures audioFeatures)
        {
            if (audioMetricsWindow == null)
            {
                audioMetricsWindow = new AudioMetricsWindow(audioFeatures);
                audioMetricsWindow.Show();
                audioMetricsWindow.Closed += (sender, args) => audioMetricsWindow = null;
            }
            else
            {
                audioMetricsWindow.UpdateData(audioFeatures);
                audioMetricsWindow.Activate();
            }
        }

        void IWindowService.ShowAudioAnalysisWindow()
        {
            if (audioAnalysisWindow == null)
            {
                audioAnalysisWindow = new();
                audioAnalysisWindow.Show();
                audioAnalysisWindow.Closed += (sender, args) => audioAnalysisWindow = null;
            }
            else
            {
                audioAnalysisWindow.Activate();
            }

        }

        bool IWindowService.IsAudioMetricsWindowOpen() => audioMetricsWindow != null;

        bool IWindowService.ShowUpdateWindowDialog() => MessageBox.Show("A new version of Mini Spotify Controller is available. Do you want to download it?", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        void IWindowService.ShowDevicesContextMenu(Device[] devices, Func<string, Task> transferPlayback)
        {
            ContextMenu contextMenu = new();

            foreach (Device device in devices)
            {
                MenuItem menuItem = new()
                {
                    Header = device.Name,
                    Tag = device.Id,
                    IsCheckable = true,
                    IsChecked = device.IsActive,
                };
                menuItem.Click += async (sender, args) =>
                {
                    if (sender is MenuItem menuItem)
                    {
                        await transferPlayback(menuItem.Tag as string ?? string.Empty);
                    }
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        void IWindowService.ShowLyricsDecisionWindow()
        {
            if (lyricsDecisionWindow == null)
            {
                lyricsDecisionWindow = new();
                lyricsDecisionWindow.Show();
                lyricsDecisionWindow.Closed += (sender, args) => lyricsDecisionWindow = null;
            }
            else
            {
                lyricsDecisionWindow.Activate();
            }
        }

        void IWindowService.ShowLyricsWindow()
        {
            if (lyricsWindow == null)
            {
                lyricsWindow = new();
                lyricsWindow.Show();
                lyricsWindow.Closed += (sender, args) => lyricsWindow = null;
            }
            else
            {
                lyricsWindow.Activate();
            }
        }

        #region Fields
        AuthWindow? authWindow;
        ClientIdWindow? clientIdWindow;
        AudioMetricsWindow? audioMetricsWindow;
        AudioAnalysisWindow? audioAnalysisWindow;
        LyricsDecisionWindowDialog? lyricsDecisionWindow;
        LyricsWindow? lyricsWindow;
        #endregion
    }
}
