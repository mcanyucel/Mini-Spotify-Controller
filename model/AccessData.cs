namespace Mini_Spotify_Controller.model
{
    internal class AccessData
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}
