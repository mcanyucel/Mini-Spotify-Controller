using MiniSpotifyController.model.Lyrics;
using System.Threading.Tasks;

namespace MiniSpotifyController.service
{
    internal interface ILyricsService
    {
        public Task<LyricsResult> GetLyrics(string songName, string artist);
    }
}
