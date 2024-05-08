using System.Windows;
using System.Windows.Controls;

namespace MiniSpotifyController.control;

/// <summary>
/// Interaction logic for AudioAnalysisItem.xaml
/// </summary>
public partial class AudioAnalysisItem : UserControl
{
    public AudioAnalysisItem()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        "Title",
        typeof(string),
        typeof(AudioAnalysisItem),
        new PropertyMetadata("Title"));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(object),
        typeof(AudioAnalysisItem),
        new PropertyMetadata("Value"));

    public static readonly DependencyProperty InformationProperty = DependencyProperty.Register(
        "Information",
        typeof(string),
        typeof(AudioAnalysisItem),
        new PropertyMetadata("Information")
        );

    internal string Title
    {
        get => GetValue(TitleProperty).ToString() ?? "Title";
        set => SetValue(TitleProperty, value);
    }

    internal string Value
    {
        get => GetValue(ValueProperty).ToString() ?? "Value";
        set => SetValue(ValueProperty, value);
    }

    internal string Information
    {
        get => GetValue(InformationProperty).ToString() ?? "Information";
        set => SetValue(InformationProperty, value);
    }
}
