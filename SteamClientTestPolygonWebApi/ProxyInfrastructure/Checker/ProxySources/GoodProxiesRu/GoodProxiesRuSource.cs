using Microsoft.Extensions.Options;

namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

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

//ToDO: Add validation
public class GoodProxiesRuSettings
{
    public string ApiKey { get; init; } = null!;
    public int MaxPingMs { get; init; }
    public int MaxTimeFromLastUpdateSeconds { get; init; }
    public int MinSuccessfulChecksPercent { get; init; }

    //public const string SectionName = "GoodProxiesRuSettings";
}