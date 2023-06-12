using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mini_Spotify_Controller.service;

namespace Mini_Spotify_Controller.viewmodel
{
    class ClientIdViewModel : ObservableObject
    {
        public string ClientId { get => m_ClientId; set { SetProperty(ref m_ClientId, value); m_SaveCommand.NotifyCanExecuteChanged(); } }
        public IRelayCommand SaveCommand { get => m_SaveCommand; }
        public ClientIdViewModel(IPreferenceService preferenceService, IWindowService windowService)
        {
            m_PreferenceService = preferenceService;
            m_WindowService = windowService;
            m_SaveCommand = new RelayCommand(Save, SaveCanExecute);
        }

        private void Save()
        {
            m_PreferenceService.SetClientId(m_ClientId);
            m_WindowService.CloseClientIdWindowDialog();

        }

        private bool SaveCanExecute() => !string.IsNullOrWhiteSpace(ClientId);


        #region Fields
        private string m_ClientId = "";
        private readonly IRelayCommand m_SaveCommand;
        private readonly IPreferenceService m_PreferenceService;
        private readonly IWindowService m_WindowService;
        #endregion
    }
}
