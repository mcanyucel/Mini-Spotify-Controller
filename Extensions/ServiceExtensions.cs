using System;

namespace MiniSpotifyController.Extensions;

public class ServiceExtensions
{
    public static int GetHash(string viewModelName, int? id = null)
    {
        return id == null ? viewModelName.GetHashCode() : HashCode.Combine(viewModelName, id);
    }
}