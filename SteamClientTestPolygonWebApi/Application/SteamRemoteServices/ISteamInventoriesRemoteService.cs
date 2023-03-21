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

public class SteamInventoriesRemoteService : ISteamInventoriesRemoteService
{
    private readonly ISteamInventoriesClient _inventoriesClient;

    public SteamInventoriesRemoteService(ISteamInventoriesClient inventoriesClient) =>
        _inventoriesClient = inventoriesClient;

    public async Task<SteamServiceResult<SteamSdkInventoryResponse?>> GetInventory(
        long steamId,
        int appId,
        int maxCount,
        CancellationToken token = default)
    {
        return await SteamApiResponseToOneOfMapper.Map(() =>
            _inventoriesClient.GetInventory(steamId, appId, maxCount, token));
    }
}


// OneOf<SteamSdkInventoryResponse?, ConnectionToSteamError, SteamError>

// https://steamcommunity.com/inventory/76561198015469433/570/2?l=english&count=5000