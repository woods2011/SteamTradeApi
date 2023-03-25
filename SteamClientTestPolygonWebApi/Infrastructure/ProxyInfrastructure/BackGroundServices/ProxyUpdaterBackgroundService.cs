using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.BackGroundServices;

public class ProxyUpdaterBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProxyUpdaterBackgroundService(IServiceScopeFactory serviceScopeFactory) =>
        _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), token);
        
        const int delayBetweenUpdates = 30; // ToDo: Move to options
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(delayBetweenUpdates));

        do await UpdateProxies(token);
        while (await timer.WaitForNextTickAsync(token));
    }

    private async Task UpdateProxies(CancellationToken token)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var proxyUpdaterService = scope.ServiceProvider.GetRequiredService<IProxyUpdaterService>();
        await proxyUpdaterService.UpdateProxies(token);
    }
}

public interface IProxyUpdaterService
{
    Task UpdateProxies(CancellationToken token);
}

public class ProxyUpdaterService : IProxyUpdaterService
{
    private readonly ISelfIpAddressProvider _selfIpAddressProvider;
    private readonly IEnumerable<IProxyUpdateConsumer> _proxyUpdateConsumers;
    private readonly IEnumerable<IProxySource> _proxySources;
    private readonly ProxyChecker _proxyChecker;
    private readonly ILogger<ProxyUpdaterService> _logger;

    public ProxyUpdaterService(
        IEnumerable<IProxyUpdateConsumer> proxyUpdateConsumers,
        IEnumerable<IProxySource> proxySources,
        ProxyChecker proxyChecker,
        ISelfIpAddressProvider selfIpAddressProvider,
        ILogger<ProxyUpdaterService> logger)
    {
        _proxyUpdateConsumers = proxyUpdateConsumers;
        _proxySources = proxySources;
        _proxyChecker = proxyChecker;
        _selfIpAddressProvider = selfIpAddressProvider;
        _logger = logger;
    }

    /// <summary>
    /// Aggregates proxies from all sources and notify all consumers
    /// </summary>
    public async Task UpdateProxies(CancellationToken token)
    {
        //_logger.LogInformation("Proxy list update Started at {Time}", DateTime.UtcNow); // ToDo: remove time

        const int requestsPerSec = 20;                   // ToDo: move to config
        const int maxSimultaneouslyRequestsCount = 350; // ToDo: move to config

        var proxiesLists = await
            _proxySources.Select(async source => await source.GetProxiesAsync(token)).WhenAll();
        var proxies = proxiesLists.SelectMany(proxies => proxies).Distinct(); // ToDo: replace with AsyncEnumerable

        await _selfIpAddressProvider.TryForceUpdateAsync(token);

        async Task<bool> ProxyIsValidPredicate(Uri proxy)
        {
            var checkResult = await _proxyChecker.CheckProxyAsync(proxy, token);
            return checkResult is { ProxyAnonymityLevel: ProxyAnonymityLevel.Anonymous or ProxyAnonymityLevel.Elite };
        }

        var validProxiesTasks = await proxies
            .RunTasksWithDelay(
                async uri => await ProxyIsValidPredicate(uri) ? uri : null,
                requestsPerSec, maxSimultaneouslyRequestsCount, token)
            .ToListAsync(token);

        var validProxies = (await validProxiesTasks.WhenAll()).WhereNotNull().ToList();

        foreach (var proxyUpdateConsumer in _proxyUpdateConsumers)
            proxyUpdateConsumer.RefreshProxyPool(validProxies);

        //_logger.LogInformation("Proxy list update Finished at {Time}", DateTime.UtcNow);
        //_logger.LogInformation("Total valid proxies Count: {ProxiesCount}", validProxies.Count);
    }
    // var validProxies = await proxies.ToAsyncEnumerable()
    //     .WhereAwait(async uri => await ProxyIsValidPredicate(uri))
    //     .ToListAsync(token);
}