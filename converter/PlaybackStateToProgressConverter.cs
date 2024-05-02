using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;
            if (value is int progressMs)
            {
                result = ((int)Math.Floor(progressMs / 1000.0));
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("PlaybackStateToProgressConverter can only be used OneWay.");
        }
    }
}
