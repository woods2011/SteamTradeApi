using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;

namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GeneralGameInventoryResponse(int AppId,
    string OwnerSteam64Id,
    DateTime LastUpdateTimeUtc,
    List<GeneralGameInventoryAssetResponse> Assets)
{
    public int TotalAssetsCount => Assets.Count;

    public void Deconstruct(out int appId, out string ownerSteam64Id, out DateTime lastUpdateTimeUtc,
        out List<GeneralGameInventoryAssetResponse> assets)
    {
        appId = this.AppId;
        ownerSteam64Id = this.OwnerSteam64Id;
        lastUpdateTimeUtc = this.LastUpdateTimeUtc;
        assets = this.Assets;
    }
}

public record GeneralGameInventoryAssetResponse(
    string AssetId,
    int AppId,
    string ItemMarketHashName,
    string OwnerSteam64Id,
    bool IsTradable,
    DateTime? TradeCooldownUntilUtc,
    bool IsMarketable,
    string InstanceId
);