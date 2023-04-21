using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Helpers.Extensions;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Market.Queries;

public class GetItemMarketHistoryQuery : IRequest<GetItemMarketHistoryResult>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public string MarketHashName { get; init; } = null!;
}

public class GetItemMarketHistoryQueryHandler :
    IRequestHandler<GetItemMarketHistoryQuery, GetItemMarketHistoryResult>
{
    private readonly ISteamMarketRemoteService _steamMarketService;
    private readonly IDistributedCache _cache;

    public GetItemMarketHistoryQueryHandler(ISteamMarketRemoteService steamMarketService, IDistributedCache cache)
    {
        _steamMarketService = steamMarketService;
        _cache = cache;
    }

    public async Task<GetItemMarketHistoryResult> Handle(
        GetItemMarketHistoryQuery request,
        CancellationToken token)
    {
        var (appId, marketHashName) = (request.AppId, request.MarketHashName);
        var entryKey = $"ItemMarketHistory-{appId}-{marketHashName}";

        var cachedSerializedResponse = await _cache.GetStringAsync(entryKey, token);
        if (cachedSerializedResponse is not null)
        {
            var response = JsonSerializer.Deserialize<GameItemMarketHistoryChartResponse>(cachedSerializedResponse);
            if (response is not null) return response;
        }

        SteamServiceResult<GameItemMarketHistoryChartResponse?> steamServiceResult = 
            await _steamMarketService.GetItemMarketHistory(appId, marketHashName, token);

        if (!steamServiceResult.TryPickResult(out GameItemMarketHistoryChartResponse? chartResponse, out var errors)) 
            return errors;

        if (chartResponse is null) return new NotFound();

        await CacheResponse(chartResponse);
        return chartResponse;

        async Task CacheResponse(GameItemMarketHistoryChartResponse chart)
        {
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(6))
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
            var serializedChart = JsonSerializer.Serialize(chart);
            await _cache.SetStringAsync(entryKey, serializedChart, cacheOptions, token);
        }
    }
}

[GenerateOneOf]
public partial class GetItemMarketHistoryResult :
    OneOfBase<GameItemMarketHistoryChartResponse, NotFound, ProxyServersError, SteamError>
{
    public static implicit operator GetItemMarketHistoryResult(OneOf<NotFound, ProxyServersError, SteamError> errors)
        => errors.Match<GetItemMarketHistoryResult>(y => y, z => z, w => w);
}