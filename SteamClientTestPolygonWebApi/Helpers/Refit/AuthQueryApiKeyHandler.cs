using System.Web;

namespace SteamClientTestPolygonWebApi.Helpers.Refit;

/// <summary>
/// Custom delegating handler for adding Api Key to the query string
/// </summary>
public class AuthQueryApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;
    public string ParamName { get; init; } = "key";

    public AuthQueryApiKeyHandler(string apiKey) => _apiKey = apiKey;

    private AuthQueryApiKeyHandler(HttpMessageHandler innerHandler, string apiKey) : this(apiKey)
        => InnerHandler = innerHandler;

    public static AuthQueryApiKeyHandler CreateInstance(HttpMessageHandler innerHandler, string apiKey)
        => new(innerHandler, apiKey);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var uriBuilder = new UriBuilder(request.RequestUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query[ParamName] = _apiKey;
        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}