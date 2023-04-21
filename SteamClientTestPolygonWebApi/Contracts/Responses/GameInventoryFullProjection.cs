namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameInventoryFullProjection(
    int AppId,
    string OwnerSteam64Id,
    DateTime LastUpdateTimeUtc,
    IReadOnlyCollection<GameInventoryAssetFullProjection> Assets)
{
    public int AppId { get; } = AppId;
    public string OwnerSteam64Id { get; } = OwnerSteam64Id;
    public DateTime LastUpdateTimeUtc { get; } = LastUpdateTimeUtc;
    public int TotalAssetsCount => Assets.Count;
    public decimal TotalPriceUsd => Assets.Sum(asset => asset.GameItem.PriceInfo?.LowestMarketPriceUsd ?? 0);
    public IReadOnlyCollection<GameInventoryAssetFullProjection> Assets { get; } = Assets;
}
// Note: Mapster conflicts (causes double left join) with Init and PrimaryCtor when computed properties are present. Ignore Prop not help.

public record GameInventoryAssetFullProjection(
    string AssetId,
    GameItemFullProjection GameItem,
    bool IsTradable,
    DateTime? TradeCooldownUntilUtc,
    bool IsMarketable,
    string InstanceId);

public record GameItemFullProjection(
    string MarketHashName,
    string IconUrl,
    string ClassId)
{
    public string MarketHashName { get; } = MarketHashName;
    public string IconUrl { get; } = IconUrl;
    public string ClassId { get; } = ClassId;
    public PriceInfoFullProjection? PriceInfo { get; init; }
}

public record PriceInfoFullProjection(
    decimal LowestMarketPriceUsd,
    decimal? MedianMarketPriceUsd,
    DateTime LastUpdateUtc);