using Mini_Spotify_Controller.model;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    internal class PlaybackStateImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlaybackState playbackState && playbackState.IsPlaying && playbackState.CurrentlyPlayingAlbum?.ImageUrl != null)
                return new Uri(playbackState.CurrentlyPlayingAlbum.ImageUrl);

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "assets", "spotify.png");
            return new Uri(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
