using System.Threading.Tasks;

namespace MiniSpotifyController.service
{
    internal interface ILyricsService
    {
        public Task<string> GetLyrics(string songName, string artist);
    }
}
