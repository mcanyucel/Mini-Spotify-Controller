using MiniSpotifyController.model.Lyrics;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MiniSpotifyController.converter
{
    internal sealed class LyricsResultTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LyricsResultType type)
            {
                return type switch
                {
                    LyricsResultType.ExactMatch => "Exact Match!",
                    LyricsResultType.Error => "Error. Sadge.",
                    LyricsResultType.SimilarMatch => "No exact match, so here is a song with a similar name.",
                    LyricsResultType.NoResult => "Wow, not a single match!",
                    LyricsResultType.NameMatch => "Same song, different artist. Coincidence? I think not!",
                    _ => throw new ArgumentOutOfRangeException(paramName: nameof(value), type, null)
                };
            }
            return "Unknown result type";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Cannot convert back");
        }
    }
}
