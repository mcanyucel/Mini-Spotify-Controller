namespace MiniSpotifyController.service
{
    interface IPreferenceService
    {
        internal void SetRefreshToken(string refreshToken);
        internal string? GetRefreshToken();
        internal string? GetClientId();
        internal void SetClientId(string clientId);
    }
}
