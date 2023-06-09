﻿namespace Mini_Spotify_Controller.service.implementation
{
    class PreferenceService : IPreferenceService
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
    }
}
