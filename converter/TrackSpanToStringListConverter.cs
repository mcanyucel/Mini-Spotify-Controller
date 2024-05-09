using MiniSpotifyController.model.AudioAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MiniSpotifyController.converter;

internal sealed class TrackSpanToStringListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable<Tatum> tatums)
        {
            return tatums.ToList().Select(tatum =>
            {
                var start = TimeSpan.FromSeconds(tatum.Start);
                var end = TimeSpan.FromSeconds(tatum.Start + tatum.Duration);
                return $"{start:mm\\:ss} - {end:mm\\:ss} ({System.Convert.ToByte(tatum.Confidence * 100)}%)";
            });
        }
        else if (value is IEnumerable<Beat> beats)
        {
            return beats.ToList().Select(beat =>
            {
                var start = TimeSpan.FromSeconds(beat.Start);
                var end = TimeSpan.FromSeconds(beat.Start + beat.Duration);
                return $"{start:mm\\:ss} - {end:mm\\:ss} ({System.Convert.ToByte(beat.Confidence * 100)}%)";
            });
        }
        else if (value is IEnumerable<Bar> bars)
        {
            return bars.ToList().Select(bar =>
            {
                var start = TimeSpan.FromSeconds(bar.Start);
                var end = TimeSpan.FromSeconds(bar.Start + bar.Duration);
                return $"{start:mm\\:ss} - {end:mm\\:ss} ({System.Convert.ToByte(bar.Confidence * 100)}%)";
            });
        }
        else if (value is IEnumerable<Segment> segments)
        {
            return segments.ToList().Select(segment =>
            {
                var start = TimeSpan.FromSeconds(segment.Start);
                var end = TimeSpan.FromSeconds(segment.Start + segment.Duration);
                return $"{start:mm\\:ss} - {end::mm\\:ss} ({System.Convert.ToByte(segment.Confidence * 100)}%)";
            });
        }
        else
        {
            return Array.Empty<string>();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new InvalidOperationException("This converter cannot convert back.");
}
