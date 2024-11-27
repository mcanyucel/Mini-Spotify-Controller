using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace MiniSpotifyController.OAuth;

public class RegistryTokenStorage(string registryKeyPath, string refreshTokenValueName) : ITokenStorage
{
    public void StoreRefreshToken(string refreshToken)
    {
        var encryptedToken = ProtectedData.Protect(Encoding.UTF8.GetBytes(refreshToken), null, DataProtectionScope.CurrentUser);
        using var key = Registry.CurrentUser.CreateSubKey(registryKeyPath);
        key.SetValue(refreshTokenValueName, Convert.ToBase64String(encryptedToken));
    }

    public string? RetrieveRefreshToken()
    {
        using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath);
        
        var value = key?.GetValue(refreshTokenValueName);
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return null;
        }
        var encryptedRefreshToken = Convert.FromBase64String(value.ToString() ?? string.Empty);
        var decryptedRefreshToken = ProtectedData.Unprotect(encryptedRefreshToken, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedRefreshToken);
    }

    public void ClearRefreshToken()
    {
        using var key = Registry.CurrentUser.OpenSubKey(registryKeyPath, true);
        key?.DeleteValue(refreshTokenValueName, false);
    }
}