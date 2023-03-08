using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

public interface ISteamInventoriesClient
{
    [Get("/inventory/{steamId}/{appId}/2?l=english")]
    Task<ApiResponse<SteamSdkInventoryResponse>> GetInventory(
        long steamId, int appId, [AliasAs("count")] int maxCount = 5000);
}

// https://steamcommunity.com/inventory/76561198015469433/570/2?l=english&count=5000
