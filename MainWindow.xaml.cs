using Mini_Spotify_Controller.viewmodel;
using System.Diagnostics;

namespace Mini_Spotify_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = App.Current.Services.GetService(typeof(MainViewModel));

        }

        

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine($"Slider_ValueChanged in Main Window {e.OldValue}, {e.NewValue}");
            Debug.WriteLine($"{e.Source}, | {e.OriginalSource}");
        }
    }
}
