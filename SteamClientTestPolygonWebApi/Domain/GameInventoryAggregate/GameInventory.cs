using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;

namespace SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;

public class GameInventory
{
    public int AppId { get; private set; }
    public string OwnerSteam64Id { get; private set; }
    public DateTime LastUpdateTimeUtc { get; private set; }
    public IReadOnlyList<GameInventoryAsset> Assets => _assets.AsReadOnly();


    private GameInventory(
        int appId,
        string ownerSteam64Id,
        DateTime lastUpdateTimeUtc,
        List<GameInventoryAsset> assets)
    {
        AppId = appId;
        OwnerSteam64Id = ownerSteam64Id;
        LastUpdateTimeUtc = lastUpdateTimeUtc;
        _assets = assets;
    }

    public static GameInventory Create(
        int appId,
        string ownerSteam64Id,
        DateTime lastUpdateDateTime,
        List<GameInventoryAsset> assets)
    {
        assets.ForEach(asset => EnforceAppIdInvariant(asset, appId));
        return new GameInventory(appId, ownerSteam64Id, lastUpdateDateTime, assets);
    }


    public void UpdateInventory(List<GameInventoryAsset> assets, DateTime lastUpdateTimeUtc)
    {
        assets.ForEach(asset => EnforceAppIdInvariant(asset, AppId));
        _assets = assets;
        LastUpdateTimeUtc = lastUpdateTimeUtc;
    }

    public void AddAsset(GameInventoryAsset asset)
    {
        EnforceAppIdInvariant(asset, AppId);
        _assets.Add(asset);
    }

    private static void EnforceAppIdInvariant(GameInventoryAsset asset, int appId)
    {
        if (asset.AppId != appId)
            throw new ArgumentException("Asset must have the same AppId as the inventory.");
    }


    private List<GameInventoryAsset> _assets = new();

#pragma warning disable CS8618
    private GameInventory() { }
#pragma warning restore CS8618
}

// AppId + OwnerSteam64Id = GameInventoryId