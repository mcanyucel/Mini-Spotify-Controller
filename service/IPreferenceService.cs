namespace Mini_Spotify_Controller.service
{
    interface IPreferenceService
    {
        internal void SetRefreshToken(string refreshToken);
        internal string? GetRefreshToken();
        internal string? GetClientId();
    }
}
