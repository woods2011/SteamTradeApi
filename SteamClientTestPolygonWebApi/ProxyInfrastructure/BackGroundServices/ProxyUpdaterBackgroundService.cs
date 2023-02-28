using SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxyAnonymityJudges;
using SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources;

namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.BackGroundServices;

public class ProxyUpdaterBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProxyUpdaterBackgroundService(IServiceScopeFactory serviceScopeFactory)
        => _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await UpdateProxies(token);

        var timer = new PeriodicTimer(TimeSpan.FromMinutes(45));
        while (await timer.WaitForNextTickAsync(token))
            await UpdateProxies(token);
    }

    private async Task UpdateProxies(CancellationToken token)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var myService = scope.ServiceProvider.GetRequiredService<IProxyUpdaterService>();
        await myService.UpdateProxies(token);
    }
}

public interface IProxyUpdaterService
{
    Task UpdateProxies(CancellationToken token);
}

public class ProxyUpdaterService : IProxyUpdaterService
{
    private readonly SelfIpAddressProvider _selfIpAddressProvider;
    private readonly IEnumerable<IProxyUpdateConsumer> _proxyUpdateConsumers;
    private readonly IEnumerable<IProxySource> _proxySources;
    private readonly ProxyChecker _proxyChecker;

    public ProxyUpdaterService(
        IEnumerable<IProxyUpdateConsumer> proxyUpdateConsumers, IEnumerable<IProxySource> proxySources,
        ProxyChecker proxyChecker, SelfIpAddressProvider selfIpAddressProvider)
    {
        _proxyUpdateConsumers = proxyUpdateConsumers;
        _proxySources = proxySources;
        _proxyChecker = proxyChecker;
        _selfIpAddressProvider = selfIpAddressProvider;
    }

    /// <summary>
    /// Aggregates proxies from all sources and notify all consumers
    /// </summary>
    public async Task UpdateProxies(CancellationToken token)
    {
        var proxiesLists = await Task.WhenAll(
            _proxySources.Select(async source => await source.GetProxiesAsync(token)));
        var proxies = proxiesLists.SelectMany(proxies => proxies).Distinct(); // ToDo: replace with AsyncEnumerable

        await _selfIpAddressProvider.TryForceUpdateAsync(token);

        async Task<bool> ProxyIsValidPredicate(Uri proxy)
        {
            var checkResult = await _proxyChecker.CheckProxyAsync(proxy, token);
            return checkResult is { ProxyAnonymityLevel: ProxyAnonymityLevel.Anonymous or ProxyAnonymityLevel.Elite };
        }

        var validProxies =
            (await Task.WhenAll(proxies.Select(async uri => await ProxyIsValidPredicate(uri) ? uri : null)))
            .OfType<Uri>().ToList();

        foreach (var proxyUpdateConsumer in _proxyUpdateConsumers)
            proxyUpdateConsumer.RefreshProxyPool(validProxies);
    }
    // var validProxies = await proxies.ToAsyncEnumerable()
    //     .WhereAwait(async uri => await ProxyIsValidPredicate(uri))
    //     .ToListAsync(token);
}