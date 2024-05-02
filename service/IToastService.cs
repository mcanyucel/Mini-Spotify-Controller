namespace MiniSpotifyController.service;

internal interface IToastService
{
    internal void ShowTextToast(string action, int conversationId, string header, string content);
}
