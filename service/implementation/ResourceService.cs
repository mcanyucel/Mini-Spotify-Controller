using MiniSpotifyController.assets;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiniSpotifyController.service.implementation
{
    internal sealed class ResourceService : IResourceService
    {
        public string GetWebPlayerPath(string accessToken)
        {
            var source = Resources.WebPlayer;
            // replace variables
            var map = new Dictionary<string, string>
            {
                { "{{internalPlayerName}}", ISpotifyService.INTERNAL_PLAYER_NAME },
                { "{{accessToken}}", accessToken }
            };
            var html = MultipleReplace(source, map);
            // write the source to a temporary file
            var tempDirectory = System.IO.Path.GetTempPath();
            var tempFile = System.IO.Path.Combine(tempDirectory, "webplayer.html");
            System.IO.File.WriteAllText(tempFile, html);
            return tempFile;
        }

        #region Helpers
        static string MultipleReplace(string text, Dictionary<string, string> map) =>
            Regex.Replace(text, string.Join("|", map.Keys.Select(Regex.Escape)), m => map[m.Value]);
        #endregion
    }
}
