namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameInventorySplitProjection(
    int AppId,
    string OwnerSteam64Id,
    DateTime LastUpdateTimeUtc,
    IReadOnlyCollection<GameInventoryAssetSplitProjection> Assets,
    IReadOnlyCollection<GameItemFullProjection> GameItems)
{
    public int AppId { get; } = AppId;
    public string OwnerSteam64Id { get; } = OwnerSteam64Id;
    public DateTime LastUpdateTimeUtc { get; } = LastUpdateTimeUtc;

    public int TotalAssetsCount => Assets.Count;
    public int TotalItemsCount => GameItems.Count;
    public IReadOnlyCollection<GameInventoryAssetSplitProjection> Assets { get; } = Assets;
    public IReadOnlyCollection<GameItemFullProjection> GameItems { get; } = GameItems;
}

public record GameInventoryAssetSplitProjection(
    string AssetId,
    string MarketHashName,
    bool IsTradable,
    DateTime? TradeCooldownUntilUtc,
    bool IsMarketable,
    string InstanceId);