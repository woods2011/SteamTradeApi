﻿using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;

namespace SteamClientTestPolygonWebApi.Application.Features.Market.Queries;

public class GetItemMarketHistoryQuery : IRequest<GetItemMarketHistoryQueryResult>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public string MarketHashName { get; init; } = null!;
}

public class GetItemMarketHistoryQueryHandler : 
    IRequestHandler<GetItemMarketHistoryQuery, GetItemMarketHistoryQueryResult>
{
    private readonly ISteamMarketRemoteService _steamMarketService;
    private readonly IDistributedCache _cache;

    public GetItemMarketHistoryQueryHandler(ISteamMarketRemoteService steamMarketService, IDistributedCache cache)
    {
        _steamMarketService = steamMarketService;
        _cache = cache;
    }

    public async Task<GetItemMarketHistoryQueryResult> Handle(
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

        var steamServiceResult = await _steamMarketService.GetItemMarketHistory(appId, marketHashName, token);

        return await steamServiceResult.Match<Task<GetItemMarketHistoryQueryResult>>(
            async chartResponse =>
            {
                if (chartResponse is null) return new NotFound();
                await CacheResponse(chartResponse);
                return chartResponse;
            },
            connectionToSteamError => Task.FromResult<GetItemMarketHistoryQueryResult>(connectionToSteamError),
            steamError => Task.FromResult<GetItemMarketHistoryQueryResult>(steamError));

        
        async Task CacheResponse(GameItemMarketHistoryChartResponse chartResponse)
        {
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(6))
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
            var serializedChart = JsonSerializer.Serialize(chartResponse);
            await _cache.SetStringAsync(entryKey, serializedChart, cacheOptions, token);
        }
    }
}
[GenerateOneOf]
public partial class GetItemMarketHistoryQueryResult :
    OneOfBase<GameItemMarketHistoryChartResponse, NotFound, ConnectionToSteamError, SteamError> { }