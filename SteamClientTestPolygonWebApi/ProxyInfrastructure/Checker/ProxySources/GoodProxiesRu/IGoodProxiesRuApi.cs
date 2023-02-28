using System.Web;
using Microsoft.Extensions.Options;
using Refit;

namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

public interface IGoodProxiesRuApi
{
    /// <param name="type">Type Of Proxy</param>
    /// <param name="pingMs">Ping in milliseconds</param>
    /// <param name="time">Time since last update</param>
    /// <param name="works">Percent of successful checks</param>
    /// <param name="count">Max count of proxies in result</param>
    /// <param name="key">API Key</param>
    /// <returns></returns>
    [Get("/get.php?type[{type}]=on&anon['anonymous']=on&anon['elite']=on")]
    Task<ApiResponse<string>> GetAnonProxies(
        string type, [AliasAs("ping")] int pingMs, int time, int works, int? count = null);

    [Get("/get.php?type[{type}]=on")]
    Task<ApiResponse<string>> GetProxies(
        string type, [AliasAs("ping")] int pingMs, int time, int works, int? count = null);

    // [Get("/get.php?type[{type}]=on&anon['transparent']=on")]
    // Task<ApiResponse<string>> GetTransparentProxies(
    //     string type, [AliasAs("ping")] int pingMs, int time, int works, int? count = null);

    // [Get("/get.php")]
    // Task<ApiResponse<string>> GetProxiesAnyType(
    //     [AliasAs("ping")] int pingMs, int time, int works, int? count = null);
}


/// <summary>
/// Custom delegating handler for adding Api Key to the query string
/// </summary>
public class AuthQueryApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public AuthQueryApiKeyHandler(IOptions<GoodProxiesRuSettings> settings) =>
        _apiKey = settings.Value.ApiKey;

    private AuthQueryApiKeyHandler(HttpMessageHandler innerHandler, string apiKey)
    {
        _apiKey = apiKey;
        InnerHandler = innerHandler;
    }

    public static AuthQueryApiKeyHandler CreateInstance(HttpMessageHandler innerHandler,
        string apiKey) => new(innerHandler, apiKey);


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var uriBuilder = new UriBuilder(request.RequestUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["key"] = _apiKey;
        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}