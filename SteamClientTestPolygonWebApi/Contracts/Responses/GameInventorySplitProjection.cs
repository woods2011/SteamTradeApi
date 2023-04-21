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

    public decimal TotalPriceUsd => Assets
        .Join(GameItems, asset => asset.ItemMarketHashName, item => item.MarketHashName, (asset, item) => (asset, item))
        .Sum(pair => pair.item.PriceInfo?.LowestMarketPriceUsd ?? 0);

    public int TotalAssetsCount => Assets.Count;
    public int TotalItemsCount => GameItems.Count;
    public IReadOnlyCollection<GameInventoryAssetSplitProjection> Assets { get; } = Assets;
    public IReadOnlyCollection<GameItemFullProjection> GameItems { get; } = GameItems;
}
// Note: Mapster conflicts (causes double left join) with Init and PrimaryCtor when computed properties are present. Ignore Prop not help.

public record GameInventoryAssetSplitProjection(
    string AssetId,
    string ItemMarketHashName,
    bool IsTradable,
    DateTime? TradeCooldownUntilUtc,
    bool IsMarketable,
    string InstanceId);