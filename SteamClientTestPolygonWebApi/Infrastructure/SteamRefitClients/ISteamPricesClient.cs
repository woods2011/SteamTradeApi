using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
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


    // ToDo: Move to a DI, add options
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 15));

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15));
}

// https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid=570&market_hash_name=The%20Abscesserator