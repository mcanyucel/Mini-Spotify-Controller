namespace MiniSpotifyController.OAuth;

public interface ITokenStorage
{
    public void StoreRefreshToken(string refreshToken);
    public string? RetrieveRefreshToken();
    public void ClearRefreshToken();
}