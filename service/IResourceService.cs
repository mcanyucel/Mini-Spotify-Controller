namespace MiniSpotifyController.service;

internal interface IResourceService
{
    string GetWebPlayerPath(string accessToken);
}
