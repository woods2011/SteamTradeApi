using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public interface ISteamInventoriesClient
{
    public async Task<SteamClientResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId, int appId, int maxCount = 5000) =>
        await SteamApiResponseToOneOfMapper.Map(() => GetInventoryInternal(steamId, appId, maxCount));


    [Get("/inventory/{steamId}/{appId}/2?l=english")]
    internal Task<ApiResponse<SteamSdkInventoryResponse>> GetInventoryInternal(
        long steamId, int appId, [AliasAs("count")] int maxCount = 5000);


    // ToDo: Move to a DI, add options
    public static readonly AsyncRetryPolicy<HttpResponseMessage> SteamInventoriesRetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> SteamInventoriesTimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
}

// OneOf<SteamSdkInventoryResponse?, ConnectionToSteamError, SteamError>

// https://steamcommunity.com/inventory/76561198015469433/570/2?l=english&count=5000