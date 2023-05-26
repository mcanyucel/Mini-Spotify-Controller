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

        private void Slider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Slider_MouseLeftButtonDown");
        }

        private void Slider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Slider_MouseLeftButtonUp");
        }
    }
}
