using Mini_Spotify_Controller.model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    internal class PlaybackStateDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 1;
            if (value is PlaybackState playbackState)
            {
                result = ((int)Math.Floor(playbackState.DurationMs / 1000.0));
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
