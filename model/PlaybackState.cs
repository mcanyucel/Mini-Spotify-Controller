using CommunityToolkit.Mvvm.ComponentModel;

namespace Mini_Spotify_Controller.model
{
    internal class PlaybackState : ObservableObject
    {
        public bool IsPlaying { get => m_IsPlaying; set => SetProperty(ref m_IsPlaying, value); }
        public string? CurrentlyPlaying { get => m_CurrentlyPlaying; set => SetProperty(ref m_CurrentlyPlaying, value); }
        public string? CurrentlyPlayingArtist { get => m_CurrentlyPlayingArtist; set => SetProperty(ref m_CurrentlyPlayingArtist, value); }
        public string? CurrentlyPlayingAlbum { get => m_CurrentlyPlayingAlbum; set => SetProperty(ref m_CurrentlyPlayingAlbum, value); }
        public string? DeviceId { get => m_DeviceId; set => SetProperty(ref m_DeviceId, value); }
        public int ProgressMs { get => m_ProgressMs; set => SetProperty(ref m_ProgressMs, value); }

        private bool m_IsPlaying;
        private string? m_CurrentlyPlaying;
        private string? m_CurrentlyPlayingArtist;
        private string? m_CurrentlyPlayingAlbum;
        private string? m_DeviceId;
        private int m_ProgressMs;
    }
}
