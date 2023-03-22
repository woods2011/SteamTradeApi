namespace SteamClientTestPolygonWebApi.Contracts.External;

public record SteamSdkItemPriceResponse(string? LowestPrice, string? Volume, string? MedianPrice);

// https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid=570&market_hash_name=The%20Abscesserator