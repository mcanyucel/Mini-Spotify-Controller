using Mini_Spotify_Controller.model;
using Mini_Spotify_Controller.window;
using System.Windows;

namespace Mini_Spotify_Controller.service.implementation
{
    class WindowService : IWindowService
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

        #region Fields
        private AuthWindow? m_AuthWindow;
        private ClientIdWindow? m_ClientIdWindow;
        private AudioMetricsWindow? m_AudioMetricsWindow;
        #endregion
    }
}
