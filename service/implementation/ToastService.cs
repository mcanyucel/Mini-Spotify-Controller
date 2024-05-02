using Microsoft.Toolkit.Uwp.Notifications;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class ToastService : IToastService
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
