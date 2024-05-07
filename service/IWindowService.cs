using MiniSpotifyController.model;
using MiniSpotifyController.model.AudioAnalysis;
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
    internal void ShowAudioFeaturesWindow(AudioFeatures audioFeatures);
    internal bool IsAudioMetricsWindowOpen();
    internal bool ShowUpdateWindowDialog();
    internal void ShowDevicesContextMenu(Device[] devices, Func<string, Task> transferPlayback);
}
