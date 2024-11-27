using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSpotifyController.service;

namespace MiniSpotifyController.viewmodel
{
    internal sealed partial class ClientIdViewModel(IPreferenceService preferenceService, IWindowService windowService) : ObservableObject, IViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _clientId = "";

        [RelayCommand(CanExecute = nameof(SaveCanExecute))]
        private void Save()
        {
            preferenceService.SetClientId(ClientId);
            windowService.CloseWindow<ClientIdViewModel>();
        }

        private bool SaveCanExecute() => !string.IsNullOrWhiteSpace(ClientId);
    }
}
