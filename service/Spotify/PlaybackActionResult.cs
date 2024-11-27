namespace MiniSpotifyController.service.Spotify;

public class PlaybackActionResult : IActionResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public IActionResult CreateSuccess() => new PlaybackActionResult(true, null);

    public IActionResult CreateFailure(string errorMessage) => new PlaybackActionResult(false, errorMessage);
    private PlaybackActionResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}