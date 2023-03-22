using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public interface ISteamPricesRemoteService
{
    Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsd(
        int appId,
        string marketHashName,
        CancellationToken token = default);

    Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
        int appId,
        IEnumerable<string> marketHashNames,
        CancellationToken token = default);
}

public class SteamPricesRemoteService : ISteamPricesRemoteService
{
    private readonly ISteamPricesClient _steamPricesClient;

    public SteamPricesRemoteService(ISteamPricesClient steamPricesClient) =>
        _steamPricesClient = steamPricesClient;

    public async Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsd(
        int appId,
        string marketHashName,
        CancellationToken token = default)
    {
        return await SteamApiResponseToOneOfMapper.Map(() =>
            _steamPricesClient.GetItemLowestMarketPriceUsd(appId, marketHashName, token));
    }

    public async Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
        int appId,
        IEnumerable<string> marketHashNames,
        CancellationToken token = default)
    {
        const int requestsPerSec = 10;                  // ToDo: move to config
        const int maxSimultaneouslyRequestsCount = 150; // ToDo: move to config

        var steamServiceResultsDelayedTasks = await marketHashNames
            .RunTasksWithDelay(
                marketHashName => GetItemLowestMarketPriceUsd(appId, marketHashName, token),
                requestsPerSec, maxSimultaneouslyRequestsCount, token)
            .ToListAsync(cancellationToken: token);

        return await steamServiceResultsDelayedTasks.WhenAllAsync();
    }
}

// public async Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
//     int appId,
//     IEnumerable<string> marketHashNames,
//     CancellationToken token = default)
// {
//     const int requestsPerSec = 10; // ToDo: move to config
//     var delayBetweenRequests = TimeSpan.FromSeconds(1) / requestsPerSec;
//     const int maxSimultaneouslyRequestsCount = 100; // ToDo: move to config
//     var semaphore = new SemaphoreSlim(maxSimultaneouslyRequestsCount);
//
//     var steamServiceResultsDelayedTasks = await marketHashNames.ToAsyncEnumerable()
//         .SelectAwait(async marketHashName =>
//         {
//             await Task.Delay(delayBetweenRequests, token);
//             await semaphore.WaitAsync(token);
//
//             return GetItemLowestMarketPriceUsd(appId, marketHashName, token)
//                 .ContinueWith(t =>
//                 {
//                     semaphore.Release();
//                     return t.Result;
//                 }, token);
//         }).ToListAsync(cancellationToken: token);
//
//     return await Task.WhenAll(steamServiceResultsDelayedTasks);
// }