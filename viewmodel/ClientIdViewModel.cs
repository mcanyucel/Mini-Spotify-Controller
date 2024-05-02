using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.service;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class ClientIdViewModel(IPreferenceService preferenceService, IWindowService windowService) : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string clientId = "";

        [RelayCommand(CanExecute = nameof(SaveCanExecute))]
        void Save()
        {
            preferenceService.SetClientId(ClientId);
            windowService.CloseClientIdWindowDialog();
        }

        private bool SaveCanExecute() => !string.IsNullOrWhiteSpace(ClientId);
    }
}
