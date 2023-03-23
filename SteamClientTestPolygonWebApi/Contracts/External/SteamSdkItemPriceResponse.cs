using System.Text.Json.Serialization;

namespace SteamClientTestPolygonWebApi.Contracts.External;

public record SteamSdkItemPriceResponse(string? LowestPrice, string? Volume, string? MedianPrice);

public record ListingsResponse(
    int TotalCount,
    [property: JsonPropertyName("listinginfo")]
    IReadOnlyDictionary<string, ListingInfoResponse> ListingInfos);

public record ListingInfoResponse(int ConvertedPricePerUnit, int ConvertedFeePerUnit);


// https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid=570&market_hash_name=The%20Abscesserator