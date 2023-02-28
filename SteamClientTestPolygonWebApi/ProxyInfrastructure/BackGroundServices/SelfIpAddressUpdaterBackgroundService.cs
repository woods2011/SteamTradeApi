using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace SteamClientTestPolygonWebApi.ProxyInfrastructure.BackGroundServices;

public class SelfIpAddressUpdaterBackgroundService : BackgroundService
{
    private readonly SelfIpAddressProvider _selfIpAddressProvider; // doesn't contain scoped services

    public SelfIpAddressUpdaterBackgroundService(SelfIpAddressProvider selfIpAddressProvider) =>
        _selfIpAddressProvider = selfIpAddressProvider;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(20));
        while (await timer.WaitForNextTickAsync(token))
            await _selfIpAddressProvider.TryForceUpdateAsync(token);
    }
}

public class SelfIpAddressProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy<HttpResponseMessage>
        .Handle<Exception>()
        .OrResult(r => r.IsSuccessStatusCode is false)
        .RetryAsync(3);

    public string Ip { get; private set; }


    //ToDo: change to async Factory method with cancellation token
    public SelfIpAddressProvider(IHttpClientFactory httpClientFactory, IOptions<IpFallBack>? ipFallBack)
    {
        _httpClientFactory = httpClientFactory;
        Ip = GetSelfIpAddressOrDefault(CancellationToken.None).Result
             ?? ipFallBack?.Value.Ip
             ?? throw new Exception("Failed to get self ip address from remote and fallback is not set");
    }

    public async Task<bool> TryForceUpdateAsync(CancellationToken token)
    {
        var ipAddressOrDefault = await GetSelfIpAddressOrDefault(token);
        if (ipAddressOrDefault is null) return false;
        Ip = ipAddressOrDefault;
        return true;
    }

    public async Task<string?> GetSelfIpAddressOrDefault(CancellationToken token)
    {
        var timeout = TimeSpan.FromSeconds(5);

        var ipifyOrgClient = new HttpClient { BaseAddress = new Uri("https://api.ipify.org"), Timeout = timeout };
        var ipifyGetIpResponse = await GetResponseOrDefault<IpifyGetIpResponse>(ipifyOrgClient, "?format=json", token);
        if (ipifyGetIpResponse is not null) return ipifyGetIpResponse.Ip;

        var ipApiComClient = new HttpClient { BaseAddress = new Uri("http://ip-api.com"), Timeout = timeout };
        var ipApiComGetIpResponse =
            await GetResponseOrDefault<IpApiComGetIpResponse>(ipApiComClient, "json/?fields=status,proxy,query", token);
        if (ipApiComGetIpResponse is not null) return ipApiComGetIpResponse.Ip;

        return null;
    }

    private async Task<TResponse?> GetResponseOrDefault<TResponse>
        (HttpClient ipifyOrgClient, string requestUri, CancellationToken token) where TResponse : class
    {
        var executeResult =
            await _retryPolicy.ExecuteAndCaptureAsync(ct => ipifyOrgClient.GetAsync(requestUri, ct), token);

        if (executeResult.Outcome is OutcomeType.Failure) return null;

        var serializedResponse = executeResult.Result;
        var response = await serializedResponse.Content.ReadFromJsonAsync<TResponse>(cancellationToken: token);
        // if (response is null)
        //     throw new Exception($"Failed to deserialize response from {ipifyOrgClient.BaseAddress?.Host}");
        return response;
    }

    private record IpifyGetIpResponse(string Ip);

    private record IpApiComGetIpResponse([property: JsonPropertyName("query")] string Ip, bool Proxy);

    public record IpFallBack(string Ip);
}