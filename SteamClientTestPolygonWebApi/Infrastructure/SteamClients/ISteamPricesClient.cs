using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

public interface ISteamPricesClient
{
    [Get("/market/priceoverview/?country=us&currency=1")]
    Task<ApiResponse<SteamSdkItemPriceResponse>> GetItemLowestMarketPrice(
        [AliasAs("appid")] int appId,
        [AliasAs("market_hash_name")] string marketHashName);
}

// https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid=570&market_hash_name=The%20Abscesserator