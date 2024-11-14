using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateToNowPlayingConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is model.PlaybackState playbackState)
            {
                return playbackState.IsPlaying ? $"{playbackState.CurrentlyPlayingArtist} - {playbackState.CurrentlyPlaying} ({playbackState.CurrentlyPlayingAlbum?.Name})" : "Nothing is playing";
            }
            else
            {
                return "Nothing is playing";
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
