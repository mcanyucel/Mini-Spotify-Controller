using System.Windows;
using System.Windows.Controls;

namespace MiniSpotifyController.window.helper
{
    internal class AudioDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AudioDataDisplayItem audioDataDisplayItem)
            {
                return audioDataDisplayItem.IsFingerprint
                    ? (DataTemplate)((FrameworkElement)container).FindResource("FingerprintDataTemplate")
                    : (DataTemplate)((FrameworkElement)container).FindResource("AudioDataTemplate");
            }
            return base.SelectTemplate(item, container);
        }
    }
}
