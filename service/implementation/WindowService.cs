using Mini_Spotify_Controller.window;

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
        #region Fields
        private AuthWindow? m_AuthWindow;
        private ClientIdWindow? m_ClientIdWindow;
        #endregion
    }
}
