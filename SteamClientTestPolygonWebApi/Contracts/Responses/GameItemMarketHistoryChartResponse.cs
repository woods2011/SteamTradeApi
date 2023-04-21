namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record GameItemMarketHistoryChartResponse(
    IEnumerable<GameItemMarketHistoryChartPointResponse> HistoryChartPointResponses);

public record GameItemMarketHistoryChartPointResponse(string DateTimeUtc, string PriceUsd, string Volume);