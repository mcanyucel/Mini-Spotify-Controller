using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MiniSpotifyController.OAuth;

public static class OAuthHelper
{
    public static void OpenBrowser(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    public static void SendResponseToBrowser(HttpListenerResponse response)
    {
        const string responseString = "<html><body><h1>Authorization successful. You can close this window.</h1></body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    public static (string codeVerifier, string codeChallenge) GenerateCodeChallengeAndVerifier()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        return (codeVerifier, codeChallenge);
    }

    public static string GenerateRandomState()
    {
        // ReSharper disable once StringLiteralTypo
        const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        const int length = 16;

        var random = new Random();
        return new string(Enumerable.Repeat(allowedChars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static byte[] ProtectData(string data)
    {
        return ProtectedData.Protect(
            Encoding.UTF8.GetBytes(data),
            null, // let's not use additional entropy for now - secret is not that sensitive
            DataProtectionScope.CurrentUser);
    }
    
    public static string UnprotectData(byte[] data)
    {
        return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser));
    }

    private static string GenerateCodeVerifier()
    {
        const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        const int minLength = 43;
        const int maxLength = 128;

        var random = new Random();
        var length = random.Next(minLength, maxLength + 1);

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[length];
        rng.GetBytes(randomBytes);

        return new string(randomBytes.Select(b => allowedChars[b % allowedChars.Length]).ToArray());
    }
    
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(challengeBytes);
    }
    
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}