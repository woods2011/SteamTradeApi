using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public interface ISteamInventoriesRemoteService
{
    public Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        CancellationToken token = default);
}

public class OfficialSteamInventoriesService : ISteamInventoriesRemoteService
{
    private readonly IOfficialSteamInventoriesClient _inventoriesClient;

    public OfficialSteamInventoriesService(IOfficialSteamInventoriesClient inventoriesClient) =>
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

        var fetchResult =
            await GeneralInventoryFetchLogic.GetInventory(steamId, appId, maxCount, FetchInventoryByPartsFunc);

        return fetchResult;
    }
}

public class SteamApisDotComUnOfficialSteamInventoriesService : ISteamInventoriesRemoteService
{
    private readonly ISteamApisDotComUnOfficialSteamInventoriesClient _inventoriesClient;

    public SteamApisDotComUnOfficialSteamInventoriesService(
        ISteamApisDotComUnOfficialSteamInventoriesClient inventoriesClient) =>
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

        var fetchResult =
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
        Func<long, int, string?,
            Task<SteamServiceResult<SteamSdkInventoryResponse?>>> fetchInventoryByPartsFunc)
    {
        var firstFetchResult = await fetchInventoryByPartsFunc(steamId, appId, null);

        if (!firstFetchResult.TryPickT0(out var accumulatedInventory, out var errors)) return firstFetchResult;
        if (accumulatedInventory is null) {return firstFetchResult;}

        var fetched = accumulatedInventory.Assets.Count;
        var totalInventoryCount = accumulatedInventory.TotalInventoryCount;

        while (fetched < Math.Min(totalInventoryCount, maxCount))
        {
            var fetchResult = await fetchInventoryByPartsFunc(steamId, appId, accumulatedInventory.LastAssetId);

            if (!fetchResult.TryPickT0(out var newInventoryPart, out _)) return accumulatedInventory;
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