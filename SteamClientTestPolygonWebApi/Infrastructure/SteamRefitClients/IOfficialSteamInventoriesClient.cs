using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

public interface IOfficialSteamInventoriesClient
{
    [Get("/inventory/{steamId}/{appId}/2?l=english")]
    public Task<ApiResponse<SteamSdkInventoryResponse>> GetInventory(
        long steamId,
        int appId,
        [AliasAs("start_assetid")] string? startAssetId = null,
        [AliasAs("count")] int maxCount = 2000,
        CancellationToken token = default);


    // ToDo: Move to a DI, add options
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 5));

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}

public interface ISteamApisDotComUnOfficialSteamInventoriesClient
{
    [Get("/steam/inventory/76561198015469433/570/2?")]
    public Task<ApiResponse<SteamSdkInventoryResponse>> GetInventory(
        [AliasAs("steamid")] long steamId,
        [AliasAs("appid")] int appId,
        [AliasAs("start_assetid")] string? startAssetId = null,
        CancellationToken token = default);

    
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(0.5), 5));

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15));
}
// https://api.steamapis.com/steam/inventory/76561198015469433/570/2?api_key=8eNrfJoHLFzws8HhCSuFMLx8oPg&start_assetid=22963835041


// public interface ISteamApiInventoriesClient
// {
//     [Get("/IEconService/GetInventoryItemsWithDescriptions/v1/?contextid=2&get_descriptions=true&language=english")]
//     public Task<ApiResponse<WrappedResponse<SteamSdkInventoryResponse>>> GetInventory(
//         [AliasAs("steamid")] long steamId,
//         [AliasAs("appid")] int appId,
//         [AliasAs("count")] int maxCount = 2000,
//         CancellationToken token = default);
//
//     public static readonly AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy =
//         Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
//
//     // https://api.steampowered.com/IEconService/GetInventoryItemsWithDescriptions/v1/?key=C275A0BDD2B4672E9D5FCEC73407B409&steamid=76561198015469433&appid=570&contextid=2&get_descriptions=true&count=5000
// }