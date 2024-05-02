using CommunityToolkit.Mvvm.ComponentModel;
using MiniSpotifyController.model;

namespace MiniSpotifyController.viewmodel
{
    internal sealed class AudioMetricsViewModel : ObservableObject
    {
        public AudioFeatures? AudioFeatures { get => m_AudioFeatures; private set => SetProperty(ref m_AudioFeatures, value); }
        public AudioAnalysis? AudioAnalysis { get => m_AudioAnalysis; private set => SetProperty(ref m_AudioAnalysis, value); }

        public void UpdateData(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            AudioAnalysis = audioAnalysis;
            AudioFeatures = audioFeatures;
        }


        #region Fields
        private AudioFeatures? m_AudioFeatures;
        private AudioAnalysis? m_AudioAnalysis;
        #endregion
    }
}
