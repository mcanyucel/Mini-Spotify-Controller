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

        void IWindowService.SetClipboardText(string text)
        {
            Clipboard.SetText(text);
        }

        void IWindowService.ShowAudioMetricsWindow(string songTitle, AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            if (m_AudioMetricsWindow == null)
            {
                m_AudioMetricsWindow = new AudioMetricsWindow(songTitle, audioFeatures, audioAnalysis);
                m_AudioMetricsWindow.Show();
                m_AudioMetricsWindow.Closed += (sender, args) => m_AudioMetricsWindow = null;
            }
            else
            {
                m_AudioMetricsWindow.UpdateData(songTitle, audioFeatures, audioAnalysis);
                m_AudioMetricsWindow.Activate();
            }
        }

        bool IWindowService.IsAudioMetricsWindowOpen() => m_AudioMetricsWindow != null;
        
        
        #region Fields
        private AuthWindow? m_AuthWindow;
        private ClientIdWindow? m_ClientIdWindow;
        private AudioMetricsWindow? m_AudioMetricsWindow;
        #endregion
    }
}
