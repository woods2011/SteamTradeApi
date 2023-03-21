namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameInventoryTradabilityProjection(
    int AppId,
    string OwnerSteam64Id,
    DateTime LastUpdateTimeUtc,
    IReadOnlyCollection<GameInventoryAssetTradabilityProjection> Assets)
{
    public int AppId { get; } = AppId;
    public string OwnerSteam64Id { get; } = OwnerSteam64Id;
    public DateTime LastUpdateTimeUtc { get; } = LastUpdateTimeUtc;

    public int TotalAssetsCount => Assets.Count;
    public IReadOnlyCollection<GameInventoryAssetTradabilityProjection> Assets { get; } = Assets;
}

public record GameInventoryAssetTradabilityProjection(
    string AssetId,
    bool IsTradable,
    DateTime? TradeCooldownUntilUtc);