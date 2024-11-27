namespace MiniSpotifyController.service.Spotify;

public interface IActionResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    IActionResult CreateSuccess();
    IActionResult CreateFailure(string errorMessage);

}