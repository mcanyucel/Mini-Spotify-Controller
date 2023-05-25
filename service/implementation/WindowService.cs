using System;
using Mini_Spotify_Controller.window;

namespace Mini_Spotify_Controller.service.implementation
{
    class WindowService : IWindowService
    {
        public string ShowAuthorizationWindow()
        {
            m_AuthWindow = new AuthWindow();
            m_AuthWindow.ShowDialog();
            return "";
        }

        public void ShowClientIdWindow()
        {
            throw new NotImplementedException();
        }

        #region Fields
        private AuthWindow? m_AuthWindow;
        #endregion
    }
}
