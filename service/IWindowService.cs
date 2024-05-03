using MiniSpotifyController.model;
using System;
using System.Threading.Tasks;

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

    internal void OpenInternalPlayer(string playerHTML);
    internal void CloseInternalPlayer();
    internal void ShowDevicesContextMenu(Device[] devices, Func<string, Task> transferPlayback);
}
