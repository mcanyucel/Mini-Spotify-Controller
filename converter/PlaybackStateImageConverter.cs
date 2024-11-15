using MiniSpotifyController.model;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using PlaybackState = MiniSpotifyController.model.Spotify.PlaybackState;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PlaybackState { IsPlaying: true,Track.Album.Images: not null } playbackState)
                return new Uri(playbackState.Track.Album.Images.FirstOrDefault()?.Url ?? string.Empty);

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "assets", "spotify.png");
            return new Uri(path);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
