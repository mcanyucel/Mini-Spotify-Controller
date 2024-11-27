using MahApps.Metro.IconPacks;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class PlaybackStateButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(q => q == DependencyProperty.UnsetValue))
            {
                return PackIconMaterialKind.Play;
            }
            
            var isBusy = (bool)values[0];
            var isPlaying = (bool)values[1];
            if (isBusy)
                return PackIconMaterialKind.TimerSand;
            return isPlaying ? PackIconMaterialKind.Pause : PackIconMaterialKind.Play;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("PlaybackStateButtonConverter can only be used OneWay.");
        }
    }
}
