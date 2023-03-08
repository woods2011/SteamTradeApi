using Refit;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

public interface IGoodProxiesRuApi
{
    /// <param name="type">Type Of Proxy</param>
    /// <param name="pingMs">Ping in milliseconds</param>
    /// <param name="time">Time since last update</param>
    /// <param name="works">Percent of successful checks</param>
    /// <param name="count">Max count of proxies in result</param>
    /// <returns>List of proxies divided by new line</returns>
    [Get("/get.php?type[{type}]=on&anon['anonymous']=on&anon['elite']=on")]
    Task<ApiResponse<string>> GetAnonProxies(
        string type, [AliasAs("ping")] int pingMs, int time, int works, int? count = null);


    /// <param name="type">Type Of Proxy</param>
    /// <param name="pingMs">Ping in milliseconds</param>
    /// <param name="time">Time since last update</param>
    /// <param name="works">Percent of successful checks</param>
    /// <param name="count">Max count of proxies in result</param>
    /// <returns>List of proxies divided by new line</returns>
    [Get("/get.php?type[{type}]=on")]
    Task<ApiResponse<string>> GetProxies(
        string type, [AliasAs("ping")] int pingMs, int time, int works, int? count = null);
}