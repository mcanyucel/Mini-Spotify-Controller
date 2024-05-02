using MiniSpotifyController.model;
using MiniSpotifyController.window;
using System.Windows;
using System.Windows.Controls;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class WindowService : IWindowService
    {
        void IWindowService.ShowClientIdWindowDialog()
        {
            m_ClientIdWindow = new ClientIdWindow();
            m_ClientIdWindow.ShowDialog();
        }
        void IWindowService.CloseClientIdWindowDialog()
        {
            m_ClientIdWindow?.Close();
            m_ClientIdWindow = null;
        }
        void IWindowService.ShowAuthorizationWindowDialog()
        {
            m_AuthWindow = new AuthWindow();
            m_AuthWindow.ShowDialog();
        }
        void IWindowService.CloseAuthorizationWindowDialog()
        {
            m_AuthWindow?.Close();
            m_AuthWindow = null;
        }

        void IWindowService.SetClipboardText(string text) => Clipboard.SetText(text);

        void IWindowService.ShowAudioMetricsWindow(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            if (m_AudioMetricsWindow == null)
            {
                m_AudioMetricsWindow = new AudioMetricsWindow(audioFeatures, audioAnalysis);
                m_AudioMetricsWindow.Show();
                m_AudioMetricsWindow.Closed += (sender, args) => m_AudioMetricsWindow = null;
            }
            else
            {
                m_AudioMetricsWindow.UpdateData(audioFeatures, audioAnalysis);
                m_AudioMetricsWindow.Activate();
            }
        }

        bool IWindowService.IsAudioMetricsWindowOpen() => m_AudioMetricsWindow != null;

        bool IWindowService.ShowUpdateWindowDialog() => MessageBox.Show("A new version of Mini Spotify Controller is available. Do you want to download it?", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        void IWindowService.ShowDevicesContextMenu(Device[] devices)
        {
            ContextMenu contextMenu = new();

            foreach (Device device in devices)
            {
                MenuItem menuItem = new()
                {
                    Header = device.Name,
                    Tag = device.Id,
                    IsCheckable = true,
                    IsChecked = device.IsActive 
                };
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.IsOpen = true;
        }

        #region Fields
        private AuthWindow? m_AuthWindow;
        private ClientIdWindow? m_ClientIdWindow;
        private AudioMetricsWindow? m_AudioMetricsWindow;
        #endregion
    }
}
