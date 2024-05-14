namespace MiniSpotifyController.service;

internal interface IPreferenceService
{
    internal void SetRefreshToken(string refreshToken);
    internal string? GetRefreshToken();
    internal string? GetClientId();
    internal void SetClientId(string clientId);
    internal string? GetGeniusClientId();
    internal string? GetGeniusAccessToken();
}
