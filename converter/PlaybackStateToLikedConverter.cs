using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Mini_Spotify_Controller.converter
{
    class PlaybackStateToLikedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is model.PlaybackState playbackState)
                return playbackState.IsLiked ? Color.Red : Color.LightGray;
            else
                throw new ArgumentException("Value is not of type PlaybackState");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("PlaybackStateToLikedConverter can only be used OneWay.");
        }
    }
}
