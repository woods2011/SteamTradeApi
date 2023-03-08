namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;

public interface IProxySource
{
    Task<IEnumerable<Uri>> GetProxiesAsync(CancellationToken token);
}