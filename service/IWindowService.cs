namespace Mini_Spotify_Controller.service
{
    interface IWindowService
    {
        internal void ShowClientIdWindowDialog();
        internal void CloseClientIdWindowDialog();
        internal void ShowAuthorizationWindowDialog();
        internal void CloseAuthorizationWindowDialog();

    }
}
