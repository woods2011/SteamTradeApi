namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker;

public static class ProxyParser
{
    public static Uri Parse(string proxy)
    {
        var uri = new Uri(proxy, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri) return uri;

        throw new ArgumentException("Uri is in the wrong format");
    }

    public static Uri Parse(string proxy, string? scheme)
    {
        if (scheme is null) return Parse(proxy);

        var uri = new Uri(proxy, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri)
            if (uri.Scheme == scheme) return uri;
            else
                throw new ArgumentException($"Input Uri scheme is not equal to specified scheme, " +
                                            $"specified scheme: {scheme}, but uri scheme was: {uri.Scheme}");

        uri = new Uri($"{scheme}{Uri.SchemeDelimiter}{uri}");
        if (uri.IsAbsoluteUri) return uri;

        throw new ArgumentException("Uri is in the wrong format");
    }
    
    public static Uri? TryParseOrDefault(string proxy, string? scheme)
    {
        if (scheme is null) return Parse(proxy);

        var uri = new Uri(proxy, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri) 
            return uri.Scheme == scheme ? uri : null;

        try
        {
            uri = new Uri($"{scheme}{Uri.SchemeDelimiter}{uri}");
        }
        catch (Exception)
        {
            return null;
        }
        
        return uri.IsAbsoluteUri ? uri : null;
    }

    // public static Uri Parse(string proxy, string? fallBackScheme = null)
    // {
    //     var uri = new Uri(proxy, UriKind.RelativeOrAbsolute);
    //     if (uri.IsAbsoluteUri) return uri;
    //     if (fallBackScheme is null)
    //         throw new ArgumentException("Scheme is not specified and no fallback scheme provided");
    //     uri = new Uri($"{fallBackScheme}{Uri.SchemeDelimiter}{uri}");
    //     if (uri.IsAbsoluteUri) return uri;
    //     throw new ArgumentException("Uri is not absolute");
    // }
}