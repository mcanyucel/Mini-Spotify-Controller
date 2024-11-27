namespace MiniSpotifyController.OAuth;

public class OAuthConfig
{
    public string AuthBaseUrl { get; init; } = "";
    public string? ClientId { get; set; } = "";
    public string RedirectUrl { get; init; } = "";
    public string Scopes { get; init; } = "";
    public string TokenUrl { get; init; } = "";
    public string RevokeUrl { get; init; } = "";
    public string State { get; } = OAuthHelper.GenerateRandomState();
}