using System.Net;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

public interface ISteamPricesClient
{
    [Get("/market/priceoverview/?country=us&currency=1")]
    Task<ApiResponse<SteamSdkItemPriceResponse>> GetItemLowestMarketPriceUsd(
        [AliasAs("appid")] int appId,
        [AliasAs("market_hash_name")] string marketHashName,
        CancellationToken token = default);
    
    
    [Get("/market/listings/{appId}/{marketHashName}/render/")]
    Task<ApiResponse<ListingsResponse>> GetItemMarketListings(
        int appId,
        string marketHashName,
        string? filter = null,
        int start = 0,
        int count = 10,
        int currency = 1,
        CancellationToken token = default);
    
    
    [Get("/market/listings/{appId}/{marketHashName}")]
    Task<ApiResponse<string>> GetItemMarketListingsWithHistoryRaw(
        int appId,
        string marketHashName,
        CancellationToken token = default);
    
    
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
            .RetryAsync(12);

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(8));
}

// https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid=570&market_hash_name=The%20Abscesserator

