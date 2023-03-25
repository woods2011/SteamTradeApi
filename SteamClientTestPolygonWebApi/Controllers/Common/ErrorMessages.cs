using SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;

namespace SteamClientTestPolygonWebApi.Controllers.Common;

public static class ErrorMessages
{
    public const string InventoryNotFound = "Inventory not found or Hidden by privacy settings";
    public const string ItemNotFoundOnMarket = "Item not found on market, please check if you typed Item name correctly";
    public const string InventoryNotLoaded = "Inventory not found, Please load it first";
    public const string ProxyServersError = "Our Proxies Servers are temporary unavailable or overloaded or Steam is down";
    public static string SteamError(SteamError error) => $"Steam Response Status Code: {(int) error.StatusCode}; Error Reason: {error.ReasonPhrase}";
}