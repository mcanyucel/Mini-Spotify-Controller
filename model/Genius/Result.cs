using System.Text.Json.Serialization;

namespace MiniSpotifyController.model.Genius;

internal sealed record Result(

    [property: JsonPropertyName("id")]
    long Id,
    [property: JsonPropertyName("api_path")]
    string ApiPath,
    [property: JsonPropertyName("primary_artist")]
    Artist PrimaryArtist,
    [property: JsonPropertyName("artist_names")]
    string ArtistNames,
    [property: JsonPropertyName("title")]
    string Title,
    [property: JsonPropertyName("url")]
    string Url,
    [property: JsonPropertyName("stats")]
    Stats Stats
);
