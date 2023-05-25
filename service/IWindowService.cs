namespace Mini_Spotify_Controller.service
{
    interface IWindowService
    {
        internal void ShowClientIdWindow();
        internal void CloseClientIdWindow();
        internal string ShowAuthorizationWindow();

    }
}
