namespace MiniSpotifyController.service.implementation
{
    internal sealed class PreferenceService : IPreferenceService
    {
        void IPreferenceService.SetClientId(string clientId)
        {
            Properties.Settings.Default.ClientId = clientId;
            Properties.Settings.Default.Save();
        }

        string? IPreferenceService.GetClientId()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ClientId))
            {
                return null;
            }
            else
            {
                return Properties.Settings.Default.ClientId;
            }
        }

        string? IPreferenceService.GetRefreshToken()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.RefreshToken))
            {
                return null;
            }
            else
            {
                return Properties.Settings.Default.RefreshToken;
            }
        }

        void IPreferenceService.SetRefreshToken(string refreshToken)
        {
            Properties.Settings.Default.RefreshToken = refreshToken;
            Properties.Settings.Default.Save();
        }

        string? IPreferenceService.GetGeniusClientId()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.GeniusClientId))
                return null;
            else
                return Properties.Settings.Default.GeniusClientId;
        }

        string? IPreferenceService.GetGeniusAccessToken()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.GeniusClientAccessToken))
                return null;
            else
                return Properties.Settings.Default.GeniusClientAccessToken;
        }
    }
}
