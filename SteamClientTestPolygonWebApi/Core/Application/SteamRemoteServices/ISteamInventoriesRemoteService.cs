using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;

public interface ISteamInventoriesRemoteService
{
    public Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        CancellationToken token = default);
}

public class SteamInventoriesService : ISteamInventoriesRemoteService
{
    private readonly ISteamInventoriesClient _inventoriesClient;

    public SteamInventoriesService(ISteamInventoriesClient inventoriesClient) =>
        _inventoriesClient = inventoriesClient;

    public async Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        CancellationToken token = default)
    {
        const int fetchPerTime = 2000;

        async Task<SteamServiceResult<SteamSdkInventoryResponse?>> FetchInventoryByPartsFunc(
            long inSteamId,
            int inAppId,
            string? lastAssetId)
        {
            return await SteamApiResponseToOneOfMapper.Map(() =>
                _inventoriesClient.GetInventory(inSteamId, inAppId, lastAssetId, fetchPerTime, token));
        }

        SteamServiceResult<SteamSdkInventoryResponse?> fetchResult =
            await GeneralInventoryFetchLogic.GetInventory(steamId, appId, maxCount, FetchInventoryByPartsFunc);

        return fetchResult;
    }
}

public class SteamApisDotComInventoriesService : ISteamInventoriesRemoteService
{
    private readonly ISteamApisDotComInventoriesClient _inventoriesClient;

    public SteamApisDotComInventoriesService(
        ISteamApisDotComInventoriesClient inventoriesClient) =>
        _inventoriesClient = inventoriesClient;

    public async Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        CancellationToken token = default)
    {
        async Task<SteamServiceResult<SteamSdkInventoryResponse?>> FetchInventoryByPartsFunc(
            long inSteamId,
            int inAppId,
            string? lastAssetId)
        {
            return await SteamApiResponseToOneOfMapper.Map(() =>
                _inventoriesClient.GetInventory(inSteamId, inAppId, lastAssetId, token));
        }

        SteamServiceResult<SteamSdkInventoryResponse?> fetchResult =
            await GeneralInventoryFetchLogic.GetInventory(steamId, appId, maxCount, FetchInventoryByPartsFunc);

        return fetchResult;
    }
}

public static class GeneralInventoryFetchLogic
{
    public static async Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        Func<long, int, string?, Task<SteamServiceResult<SteamSdkInventoryResponse?>>> fetchInventoryByPartsFunc)
    {
        SteamServiceResult<SteamSdkInventoryResponse?> firstFetchResult =
            await fetchInventoryByPartsFunc(steamId, appId, null);

        if (!firstFetchResult.TryPickResult(out SteamSdkInventoryResponse? accumulatedInventory, out var errors))
            return errors;

        if (accumulatedInventory is null) return firstFetchResult;

        var fetched = accumulatedInventory.Assets.Count;
        var totalInventoryCount = accumulatedInventory.TotalInventoryCount;

        while (fetched < Math.Min(totalInventoryCount, maxCount))
        {
            SteamServiceResult<SteamSdkInventoryResponse?> fetchResult =
                await fetchInventoryByPartsFunc(steamId, appId, accumulatedInventory.LastAssetId);

            if (!fetchResult.TryPickResult(out SteamSdkInventoryResponse? newInventoryPart, out _))
                return accumulatedInventory;
            if (newInventoryPart is null) return accumulatedInventory;

            accumulatedInventory.Merge(newInventoryPart);
            fetched += newInventoryPart.Assets.Count;
        }

        accumulatedInventory.Trim(maxCount);
        return accumulatedInventory;
    }
}

// OneOf<SteamSdkInventoryResponse?, ConnectionToSteamError, SteamError>

// https://steamcommunity.com/inventory/76561198015469433/570/2?l=english&count=2000