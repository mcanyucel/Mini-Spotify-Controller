using MiniSpotifyController.model.AudioAnalysis;
using MiniSpotifyController.window.helper;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter;

internal sealed class AudioAnalysisDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Meta meta)
        {
            return meta.ToDisplayItems();
        }
        else if (value is Track track)
        {
            return track.ToDisplayItems();
        }
        else
        {
            throw new ArgumentException("Value is not of type Meta or Track.");
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException("This converter cannot convert back.");
}
