using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

public class GoodProxiesRuSource : IProxySource
{
    private readonly IGoodProxiesRuApi _api;
    private readonly GoodProxiesRuSettings _settings;

    public GoodProxiesRuSource(IGoodProxiesRuApi api, IOptions<GoodProxiesRuSettings> settings)
    {
        _api = api;
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<IEnumerable<Uri>> GetProxiesAsync(CancellationToken token)
    {
        var proxyTypes = new[] { "http", "socks4", "socks5" };

        var proxies = new List<Uri>();
        foreach (var proxyType in proxyTypes)
        {
            var response = await _api.GetAnonProxies(
                type: proxyType,
                pingMs: _settings.MaxPingMs,
                time: _settings.MaxTimeFromLastUpdateSeconds,
                works: (100 - _settings.MinSuccessfulChecksPercent)
            );
            var proxyList = response.Content?.Split(new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            proxies.AddRange(proxyList?.Distinct()
                                 .Select(x => ProxyParser.TryParseOrDefault(x, proxyType)).OfType<Uri>()
                             ?? Array.Empty<Uri>());
        }

        return proxies;
    }
}

public class GoodProxiesRuSettings
{
    [Range(0, 20000)]
    public int MaxPingMs { get; init; }
    
    [Range(1, 600)]
    public int MaxTimeFromLastUpdateSeconds { get; init; }
    
    [Range(0, 100)]
    public int MinSuccessfulChecksPercent { get; init; }

    //public const string SectionName = "GoodProxiesRuSettings";
}