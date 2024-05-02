using MahApps.Metro.IconPacks;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isBusy = (bool)values[0];
            var isPlaying = (bool)values[1];
            if (isBusy)
                return PackIconMaterialKind.TimerSand;
            else
            {
                if (isPlaying)
                    return PackIconMaterialKind.Pause;
                else
                    return PackIconMaterialKind.Play;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("PlaybackStateButtonConverter can only be used OneWay.");
        }
    }
}
