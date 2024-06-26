﻿using MiniSpotifyController.model;
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
    internal void ShowAudioAnalysisWindow();
    internal bool IsAudioMetricsWindowOpen();
    internal bool ShowUpdateWindowDialog();
    internal void ShowDevicesContextMenu(Device[] devices, Func<string, Task> transferPlayback);
    internal void ShowLyricsWindow();


}
