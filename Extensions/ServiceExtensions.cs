using System;

namespace MiniSpotifyController.Extensions;

public static class ServiceExtensions
{
    public static int GetHash(string viewModelName, string? id = null)
    {
        return id == null ? viewModelName.GetHashCode() : HashCode.Combine(viewModelName, id);
    }
}