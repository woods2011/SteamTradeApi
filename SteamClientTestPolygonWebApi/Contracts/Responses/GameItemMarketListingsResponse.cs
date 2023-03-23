namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameItemMarketListingsResponse(IReadOnlyList<decimal> ListingsUsdPrices, int TotalListingsCount)
{
    public IReadOnlyList<decimal> ListingsUsdPrices { get; } = ListingsUsdPrices;
    public int FetchedCount => ListingsUsdPrices.Count;
    public int TotalListingsCount { get; } = TotalListingsCount;
}