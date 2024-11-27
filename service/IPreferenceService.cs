namespace MiniSpotifyController.service;

public interface IPreferenceService
{
    public string? GetClientId();
    public void SetClientId(string clientId);
    public string? GetGeniusClientId();
    public string? GetGeniusAccessToken();
}
