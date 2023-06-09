﻿using Mini_Spotify_Controller.model;
using Mini_Spotify_Controller.viewmodel;

namespace Mini_Spotify_Controller.window
{
    /// <summary>
    /// Interaction logic for AudioMetricsWindow.xaml
    /// </summary>
    public partial class AudioMetricsWindow
    {
        private readonly AudioMetricsViewModel? m_ViewModel;
        public AudioMetricsWindow(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            m_ViewModel = App.Current.Services.GetService(typeof(AudioMetricsViewModel)) as AudioMetricsViewModel;
            m_ViewModel?.UpdateData(audioFeatures, audioAnalysis);
            this.DataContext = m_ViewModel;
            InitializeComponent();
        }

        public void UpdateData(AudioFeatures audioFeatures, AudioAnalysis audioAnalysis)
        {
            m_ViewModel?.UpdateData(audioFeatures, audioAnalysis);
        }
    }
}
