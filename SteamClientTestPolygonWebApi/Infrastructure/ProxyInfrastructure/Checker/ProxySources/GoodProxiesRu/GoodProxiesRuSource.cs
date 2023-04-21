using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Refit;
using SteamClientTestPolygonWebApi.Helpers.Extensions;

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
        var proxyTypes = SupportedProxiesSchemes.All;
        var proxies = new List<Uri>();

        foreach (var proxyType in proxyTypes)
        {
            ApiResponse<string> response = await _api.GetAnonProxies(
                type: proxyType,
                pingMs: _settings.MaxPingMs,
                time: _settings.MaxTimeFromLastUpdateSeconds,
                works: (100 - _settings.MinSuccessfulChecksPercent)
            );

            var proxiesOfCurrentType = response.Content?.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var parsedProxiesOfCurrentType = proxiesOfCurrentType?.Distinct()
                .Select(uri => ProxyParser.TryParseOrDefault(uri, proxyType))
                .WhereNotNull();

            proxies.AddRange(parsedProxiesOfCurrentType ?? Array.Empty<Uri>());
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