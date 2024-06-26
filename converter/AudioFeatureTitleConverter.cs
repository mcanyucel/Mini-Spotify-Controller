﻿using MiniSpotifyController.model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class AudioFeatureTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AudioFeature audioFeature)
                return audioFeature.ToString();
            else
                return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("AudioFeatureTitleConverter can only be used OneWay.");
        }
    }
}
