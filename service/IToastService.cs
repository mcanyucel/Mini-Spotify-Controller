namespace Mini_Spotify_Controller.service
{
    interface IToastService
    {
        internal void ShowTextToast(string action, int conversationId, string header, string content);
    }
}
