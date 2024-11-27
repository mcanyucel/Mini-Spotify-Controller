using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MiniSpotifyController.model.Spotify;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateToNowPlayingConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PlaybackState playbackState)
            {
                var artistName = playbackState.Track?.Artists?.FirstOrDefault()?.Name;
                var trackName = playbackState.Track?.Name;
                var albumName = playbackState.Track?.Album?.Name;
                return playbackState.IsPlaying
                    ? $"{artistName} - {trackName} from {albumName}"
                    : "Nothing is playing";
            }

            return "Nothing is playing";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
