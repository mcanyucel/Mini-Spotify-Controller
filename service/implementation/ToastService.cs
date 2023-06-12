using Microsoft.Toolkit.Uwp.Notifications;

namespace Mini_Spotify_Controller.service.implementation
{
    class ToastService : IToastService
    {
        public static void Dispose() => ToastNotificationManagerCompat.Uninstall();
        void IToastService.ShowTextToast(string action, int conversationId, string header, string content)
        {
            new ToastContentBuilder()
                .AddArgument("action", action)
                .AddArgument("conversationId", conversationId)
                .AddText(header)
                .AddText(content)
                .Show();
        }
    }
}
