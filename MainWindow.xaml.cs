using Mini_Spotify_Controller.viewmodel;

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
            DataContext = App.Current.Services.GetService(typeof(MainViewModel));
        }
    }
}
