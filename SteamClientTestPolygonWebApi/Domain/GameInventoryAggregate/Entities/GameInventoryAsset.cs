using Newtonsoft.Json;
using SteamClientTestPolygonWebApi.Domain.Item;

namespace SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;

public class GameInventoryAsset
{
    public string AssetId { get; private set; }
    public string OwnerSteam64Id { get; private set; }
    public int AppId { get; private set; }
    public string ItemMarketHashName { get; private set; }

    public GameItem GameItem { get; private set; }

    public bool IsTradable { get; private set; }
    public DateTime? TradeCooldownUntilUtc { get; private set; }
    public bool IsMarketable { get; private set; }
    public string InstanceId { get; private set; }


    private GameInventoryAsset(
        string assetId,
        string ownerSteam64Id,
        int appId,
        string itemMarketHashName,
        bool isTradable,
        DateTime? tradeCooldownUntilUtc,
        bool isMarketable,
        string instanceId)
    {
        AssetId = assetId;
        OwnerSteam64Id = ownerSteam64Id;
        AppId = appId;
        ItemMarketHashName = itemMarketHashName;
        IsTradable = isTradable;
        TradeCooldownUntilUtc = tradeCooldownUntilUtc;
        IsMarketable = isMarketable;
        InstanceId = instanceId;
    }

    public static GameInventoryAsset Create(
        string assetId,
        string ownerSteam64Id,
        int appId,
        string itemMarketHashName,
        bool isTradable,
        DateTime? tradeCooldownUntilUtc,
        bool isMarketable,
        string instanceId)
    {
        return new GameInventoryAsset(
            assetId,
            ownerSteam64Id,
            appId,
            itemMarketHashName,
            isTradable,
            tradeCooldownUntilUtc,
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