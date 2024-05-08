using MiniSpotifyController.window.helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

    public static readonly DependencyProperty DisplayItemProperty = DependencyProperty.Register(
               "DisplayItem",
                typeof(AudioDataDisplayItem),
                typeof(AudioAnalysisItem),
                new PropertyMetadata(new AudioDataDisplayItem("Title", "Value", "Information")));

    public AudioDataDisplayItem DisplayItem
    {
        get => (AudioDataDisplayItem)GetValue(DisplayItemProperty);
        set => SetValue(DisplayItemProperty, value);
    }

    private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Clipboard.SetText(DisplayItem.Value);
        if (sender is Label label)
        {
            label.Foreground = Brushes.Green;
            label.Content = "Copied!";
        }
    }
}
