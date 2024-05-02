namespace MiniSpotifyController.service
{
    interface IToastService
    {
        internal void ShowTextToast(string action, int conversationId, string header, string content);
    }
}
