using MiniSpotifyController.model;
using System;
using System.Globalization;
using System.Windows.Data;
using PlaybackState = MiniSpotifyController.model.Spotify.PlaybackState;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateDurationConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var result = 1;
            if (value is PlaybackState playbackState)
            {
                result = (int)Math.Floor((playbackState.Track?.DurationMs ?? 0)/ 1000.0);
            }
            return result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
