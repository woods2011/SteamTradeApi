using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

public interface ISteamInventoriesClient
{
    [Get("/inventory/{steamId}/{appId}/2?l=english")]
    internal Task<ApiResponse<SteamSdkInventoryResponse>> GetInventory(
        long steamId,
        int appId,
        [AliasAs("count")] int maxCount = 5000,
        CancellationToken token = default);


    // ToDo: Move to a DI, add options
    public static readonly AsyncRetryPolicy<HttpResponseMessage> SteamInventoriesRetryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError().Or<TimeoutRejectedException>()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

    public static readonly AsyncTimeoutPolicy<HttpResponseMessage> SteamInventoriesTimeoutPolicy =
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(7));
}