namespace SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;

public class GameInventoryAsset
{
    public string AssetId { get; private set; }
    public int AppId { get; private set; }
    public string ItemMarketHashName { get; private set; }
    public string OwnerSteam64Id { get; private set; }
    public bool IsTradable { get; private set; }
    public DateTime? TradeCooldownUntilUtc { get; private set; }
    public bool IsMarketable { get; private set; }
    public string InstanceId { get; private set; }


    private GameInventoryAsset(
        string assetId,
        int appId,
        string itemMarketHashName,
        string ownerSteam64Id,
        bool isTradable,
        DateTime? tradeCooldownUntilUtc,
        bool isMarketable,
        string instanceId)
    {
        AssetId = assetId;
        AppId = appId;
        ItemMarketHashName = itemMarketHashName;
        OwnerSteam64Id = ownerSteam64Id;
        IsTradable = isTradable;
        TradeCooldownUntilUtc = tradeCooldownUntilUtc;
        IsMarketable = isMarketable;
        InstanceId = instanceId;
    }

    public static GameInventoryAsset Create(
        string assetId,
        int appId,
        string itemMarketHashName,
        string ownerSteam64Id,
        bool isTradable,
        DateTime? tradeCooldownUntil,
        bool isMarketable,
        string instanceId)
    {
        if (isTradable && tradeCooldownUntil != null)
            throw new ArgumentException("Asset cannot be tradable and have a trade cooldown at the same time.");

        return new GameInventoryAsset(
            assetId,
            appId,
            itemMarketHashName,
            ownerSteam64Id,
            isTradable,
            tradeCooldownUntil,
            isMarketable,
            instanceId);
    }


    public void AutoUpdateTradeCooldown(DateTime nowUtc)
    {
        if (IsTradable) return;
        if (TradeCooldownUntilUtc is null) return;
        if (TradeCooldownUntilUtc > nowUtc) return;

        IsTradable = true;
        TradeCooldownUntilUtc = null;
    }


#pragma warning disable CS8618
    private GameInventoryAsset() { }
#pragma warning restore CS8618
}

// AppId + AssetId = GameInventoryAssetId