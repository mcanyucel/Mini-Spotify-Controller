using CommunityToolkit.Mvvm.ComponentModel;
using Mini_Spotify_Controller.model;

namespace Mini_Spotify_Controller.viewmodel
{
    internal class AudioMetricsViewModel : ObservableObject
    {
        public AudioFeatures? AudioFeatures { get => m_AudioFeatures; private set => SetProperty(ref m_AudioFeatures, value); }
        public AudioAnalysis? AudioAnalysis { get => m_AudioAnalysis; private set => SetProperty(ref m_AudioAnalysis, value); }
        public string SongTitle { get => m_SongTitle; private set => SetProperty(ref m_SongTitle, value); }

        internal void UpdateData(string songTitle, AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            SongTitle = songTitle;
            AudioAnalysis = audioAnalysis;
            AudioFeatures = audioFeatures;
        }


        #region Fields
        private AudioFeatures? m_AudioFeatures;
        private AudioAnalysis? m_AudioAnalysis;
        private string m_SongTitle = string.Empty;
        #endregion
    }
}
