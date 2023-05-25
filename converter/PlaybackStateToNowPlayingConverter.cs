using System;
using System.Globalization;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    internal class PlaybackStateToNowPlayingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is model.PlaybackState playbackState)
            {
                if (playbackState.IsPlaying)
                {
                    return $"{playbackState.CurrentlyPlayingArtist} - {playbackState.CurrentlyPlaying} ({playbackState.CurrentlyPlayingAlbum})";
                }
                else
                {
                    return "Nothing is playing";
                }
            }
            else
            {
                return "Nothing is playing";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
