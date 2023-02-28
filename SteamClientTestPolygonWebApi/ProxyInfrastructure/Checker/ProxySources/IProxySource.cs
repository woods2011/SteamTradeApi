namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources;

public interface IProxySource
{
    Task<IEnumerable<Uri>> GetProxiesAsync(CancellationToken token);
}