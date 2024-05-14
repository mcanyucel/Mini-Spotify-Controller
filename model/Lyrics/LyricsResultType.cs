namespace MiniSpotifyController.model.Lyrics;

internal enum LyricsResultType
{
    /// <summary>
    /// An error occurred while fetching the lyrics
    /// </summary>
    Error,
    /// <summary>
    /// No result was found at all
    /// </summary>
    NoResult,
    /// <summary>
    /// Found a result with exact name but different artist
    /// </summary>
    NameMatch,
    /// <summary>
    /// Found a result with similar name (artist may be different)
    /// </summary>
    SimilarMatch,
    /// <summary>
    /// Found a result with exact name and artist
    /// </summary>
    ExactMatch
}
