using MiniSpotifyController.model;

namespace MiniSpotifyController.service;

internal interface IWindowService
{
    internal void ShowClientIdWindowDialog();
    internal void CloseClientIdWindowDialog();
    internal void ShowAuthorizationWindowDialog();
    internal void CloseAuthorizationWindowDialog();
    internal void SetClipboardText(string text);
    internal void ShowAudioMetricsWindow(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis);
    internal bool IsAudioMetricsWindowOpen();
    internal bool ShowUpdateWindowDialog();
    internal void ShowDevicesContextMenu(Device[] devices);
}
