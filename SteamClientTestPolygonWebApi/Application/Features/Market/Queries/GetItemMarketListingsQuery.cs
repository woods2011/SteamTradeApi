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

public class GetItemMarketListingsQuery : IRequest<GetItemMarketListingsResult>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public string MarketHashName { get; init; } = null!;

    public string? Filter { get; init; } = null;
}

public class GetItemMarketListingsQueryHandler :
    IRequestHandler<GetItemMarketListingsQuery, GetItemMarketListingsResult>
{
    private readonly ISteamMarketRemoteService _steamMarketService;
    private readonly IDistributedCache _cache;

    public GetItemMarketListingsQueryHandler(ISteamMarketRemoteService steamMarketService, IDistributedCache cache)
    {
        _steamMarketService = steamMarketService;
        _cache = cache;
    }

    public async Task<GetItemMarketListingsResult> Handle(
        GetItemMarketListingsQuery request,
        CancellationToken token)
    {
        var (appId, marketHashName, filter) = (request.AppId, request.MarketHashName, request.Filter);
        var entryKey = $"ItemMarketListings-{appId}-{marketHashName}-{filter ?? String.Empty}";

        var cachedSerializedResponse = await _cache.GetStringAsync(entryKey, token);
        if (cachedSerializedResponse is not null)
        {
            var cachedResponse = JsonSerializer.Deserialize<GameItemMarketListingsResponse>(cachedSerializedResponse);
            if (cachedResponse is not null) return cachedResponse;
        }

        var steamServiceResult =
            await _steamMarketService.GetItemMarketListings(appId, marketHashName, filter, token: token);

        if (!steamServiceResult.TryPickT0(out var listingsExternalResponse, out var errorsReminder))
            return errorsReminder;

        if (listingsExternalResponse is null) return new NotFound();

        var response = MapToResponse(listingsExternalResponse);
        await CacheResponse(response);
        return response;


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
public partial class GetItemMarketListingsResult :
    OneOfBase<GameItemMarketListingsResponse, NotFound, ProxyServersError, SteamError>
{
    public static implicit operator GetItemMarketListingsResult(OneOf<NotFound, ProxyServersError, SteamError> errors)
        => errors.Match<GetItemMarketListingsResult>(y => y, z => z, w => w);
}