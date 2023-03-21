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

public class GameItemFullProjection
{
    public string MarketHashName { get; init; } = null!;
    public string IconUrl { get; init; } = null!;
    public string ClassId { get; init; } = null!;
    public PriceInfoFullProjection? PriceInfo { get; init; }
}

public record PriceInfoFullProjection(
    decimal LowestMarketPriceUsd,
    decimal? MedianMarketPriceUsd,
    DateTime LastUpdateUtc);