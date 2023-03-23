using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Responses;

namespace SteamClientTestPolygonWebApi.Application.Features.Market.Queries;

public class GetItemMarketListingsQuery : IRequest<GetItemMarketListingsQueryResult>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public string MarketHashName { get; init; } = null!;

    public string? Filter { get; init; } = null;
}

public class GetItemMarketListingsQueryHandler :
    IRequestHandler<GetItemMarketListingsQuery, GetItemMarketListingsQueryResult>
{
    private readonly ISteamMarketRemoteService _steamMarketService;
    private readonly IDistributedCache _cache;

    public GetItemMarketListingsQueryHandler(ISteamMarketRemoteService steamMarketService, IDistributedCache cache)
    {
        _steamMarketService = steamMarketService;
        _cache = cache;
    }

    public async Task<GetItemMarketListingsQueryResult> Handle(
        GetItemMarketListingsQuery request,
        CancellationToken token)
    {
        var (appId, marketHashName, filter) = (request.AppId, request.MarketHashName, request.Filter);
        var entryKey = $"ItemMarketListings-{appId}-{marketHashName}-{filter ?? String.Empty}";

        var cachedSerializedResponse = await _cache.GetStringAsync(entryKey, token);
        if (cachedSerializedResponse is not null)
        {
            var response = JsonSerializer.Deserialize<GameItemMarketListingsResponse>(cachedSerializedResponse);
            if (response is not null) return response;
        }

        var steamServiceResult =
            await _steamMarketService.GetItemMarketListings(appId, marketHashName, filter, token: token);

        return await steamServiceResult.Match<Task<GetItemMarketListingsQueryResult>>(
            async listingsExternalResponse =>
            {
                if (listingsExternalResponse is null) return new NotFound();
                
                var response = MapToResponse(listingsExternalResponse);
                await CacheResponse(response);
                
                return response;
            },
            connectionToSteamError => Task.FromResult<GetItemMarketListingsQueryResult>(connectionToSteamError),
            steamError => Task.FromResult<GetItemMarketListingsQueryResult>(steamError));


        static GameItemMarketListingsResponse MapToResponse(ListingsResponse listingsExternalResponse)
        {
            var listingsUsdPrices = listingsExternalResponse.ListingInfos.Values
                .Select(pair => (pair.ConvertedFeePerUnit + pair.ConvertedPricePerUnit) / 100.0m);

            return new GameItemMarketListingsResponse(listingsUsdPrices.ToList(), listingsExternalResponse.TotalCount);
        }

        async Task CacheResponse(GameItemMarketListingsResponse listingsResponse)
        {
            var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            var serializedListingsResponse = JsonSerializer.Serialize(listingsResponse);
            await _cache.SetStringAsync(entryKey, serializedListingsResponse, cacheOptions, token);
        }
    }
}

[GenerateOneOf]
public partial class GetItemMarketListingsQueryResult :
    OneOfBase<GameItemMarketListingsResponse, NotFound, ConnectionToSteamError, SteamError> { }