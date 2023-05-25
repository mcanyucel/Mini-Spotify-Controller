using Mini_Spotify_Controller.window;

namespace Mini_Spotify_Controller.service.implementation
{
    class WindowService : IWindowService
    {

        void IWindowService.ShowClientIdWindow()
        {
            m_ClientIdWindow = new ClientIdWindow();
            m_ClientIdWindow.ShowDialog();
        }

        void IWindowService.CloseClientIdWindow()
        {
            m_ClientIdWindow?.Close();
            m_ClientIdWindow = null;
        }

        string IWindowService.ShowAuthorizationWindow()
        {
            m_AuthWindow = new AuthWindow();
            m_AuthWindow.ShowDialog();
            return "";
        }

        #region Fields
        private AuthWindow? m_AuthWindow;
        private ClientIdWindow? m_ClientIdWindow;
        #endregion
    }
}
