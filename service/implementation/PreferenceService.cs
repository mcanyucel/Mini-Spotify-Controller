using System.Security.Cryptography;
using System.Text;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class PreferenceService : IPreferenceService
    {
        public void SetClientId(string clientId)
        {
            Properties.Settings.Default.ClientId = clientId;
            Properties.Settings.Default.Save();
        }

        public string? GetClientId()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.ClientId) ? null : Properties.Settings.Default.ClientId;
        }

        public string? GetGeniusClientId()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.GeniusClientId) ? null : Properties.Settings.Default.GeniusClientId;
        }

        public string? GetGeniusAccessToken()
        {
            return string.IsNullOrWhiteSpace(Properties.Settings.Default.GeniusClientAccessToken) ? null : Properties.Settings.Default.GeniusClientAccessToken;
        }
    }
}
