using MiniSpotifyController.model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniSpotifyController.model.Spotify;
using MiniSpotifyController.viewmodel;

namespace MiniSpotifyController.service;

public interface IWindowService
{
    public void ShowWindow<TViewModel>(bool isModal = false, Dictionary<string, object>? parameters = null) where TViewModel : IViewModel;
    public void CloseWindow<TViewModel>(string? id = null) where TViewModel : IViewModel;
    internal void SetClipboardText(string text);
    internal void ShowDevicesContextMenu(Device[] devices, Func<Device, Task> transferPlayback);
    
}
