using Mini_Spotify_Controller.model;
using System;
using System.Globalization;
using System.Resources;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    internal class PlaybackStateImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlaybackState playbackState && playbackState.IsPlaying && playbackState.CurrentlyPlayingAlbum?.ImageUrl != null)
            {
                return new Uri(playbackState.CurrentlyPlayingAlbum.ImageUrl);
            }
            return new Uri("assets/white.bmp", UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
