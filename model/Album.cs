namespace Mini_Spotify_Controller.model;

/// <summary>
/// Album data
/// </summary>
/// <param name="Id">The Spotify Id of the album</param>
/// <param name="Name">The name of the album</param>
/// <param name="ImageUrl">The Spotify image url of the album</param>
public record Album(string Id, string Name, string ImageUrl)
{
    /// <summary>
    /// Empty album
    /// </summary>
    public static Album Empty => new(string.Empty, string.Empty, string.Empty);
}
