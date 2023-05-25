using Mini_Spotify_Controller.model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    internal class PlaybackStateButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PlaybackState playbackState)
            {
                if (playbackState.IsPlaying)
                {
                    return "pause";
                }
                else
                {
                    return "play";
                }
            }
            else
            {
                return "alert";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
