namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker;

public static class SupportedProxiesSchemes
{
    public const string Http = "http";
    public const string Socks4 = "socks4";
    public const string Socks5 = "socks5";

    public static string[] All => new[] {Http, Socks4, Socks5};
}