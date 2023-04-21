namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameInventoryStackPriceProjection(
    int AppId,
    string OwnerSteam64Id,
    IReadOnlyCollection<GameItemStackPriceProjection> ItemStacks)
{
    public int AppId { get; } = AppId;
    public string OwnerSteam64Id { get; } = OwnerSteam64Id;
    
    public int TotalStacksCount => ItemStacks.Count;
    public decimal TotalPriceUsd => ItemStacks.Sum(stack => stack.StackPriceUsd);

    public IReadOnlyCollection<GameItemStackPriceProjection> ItemStacks { get; } = ItemStacks;
}

public record GameItemStackPriceProjection(
    string MarketHashName,
    int Count,
    decimal ItemPriceUsd,
    string ClassId)
{
    public decimal StackPriceUsd => ItemPriceUsd * Count;

    public string MarketHashName { get; } = MarketHashName;
    public int Count { get; } = Count;
    public decimal ItemPriceUsd { get; } = ItemPriceUsd;
    public string ClassId { get; } = ClassId;
};