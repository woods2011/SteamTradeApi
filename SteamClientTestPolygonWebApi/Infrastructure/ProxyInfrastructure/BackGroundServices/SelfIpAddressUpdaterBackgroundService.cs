using System.Text.Json.Serialization;
using Polly;
using Polly.Retry;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.BackGroundServices;

public class SelfIpAddressUpdaterBackgroundService : BackgroundService
{
    private readonly ISelfIpAddressProvider _selfIpAddressProvider; // doesn't contain scoped services

    public SelfIpAddressUpdaterBackgroundService(ISelfIpAddressProvider selfIpAddressProvider) =>
        _selfIpAddressProvider = selfIpAddressProvider;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(20));
        while (await timer.WaitForNextTickAsync(token))
            await _selfIpAddressProvider.TryForceUpdateAsync(token);
    }
}

public interface ISelfIpAddressProvider
{
    string Ip { get; }
    Task<bool> TryForceUpdateAsync(CancellationToken token);
}

public class SelfIpAddressProvider : ISelfIpAddressProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _ip;

    public string Ip => _ip ??= GetSelfIpAddressOrDefault(CancellationToken.None).Result
                                ?? throw new Exception("Failed to get self ip address and fallback is not set");

    //ToDo: change to async Factory method with cancellation token
    public SelfIpAddressProvider(IHttpClientFactory httpClientFactory, IpFallBack? ipFallBack = null)
    {
        _httpClientFactory = httpClientFactory;
        _ip = ipFallBack?.Ip;
    }

    public async Task<bool> TryForceUpdateAsync(CancellationToken token)
    {
        var ipAddressOrDefault = await GetSelfIpAddressOrDefault(token);
        if (ipAddressOrDefault is null) return false;
        _ip = ipAddressOrDefault;
        return true;
    }

    private async Task<string?> GetSelfIpAddressOrDefault(CancellationToken token)
    {
        var timeout = TimeSpan.FromSeconds(5);

        var ipifyOrgClient = new HttpClient { BaseAddress = new Uri("https://api.ipify.org"), Timeout = timeout };
        var ipifyGetIpResponse = await GetResponseOrDefault<IpifyGetIpResponse>(ipifyOrgClient, "?format=json", token);
        if (ipifyGetIpResponse is not null) return ipifyGetIpResponse.Ip;

        var ipApiComClient = new HttpClient { BaseAddress = new Uri("http://ip-api.com"), Timeout = timeout };
        var ipApiComGetIpResponse =
            await GetResponseOrDefault<IpApiComGetIpResponse>(ipApiComClient, "json/?fields=status,proxy,query", token);

        return ipApiComGetIpResponse?.Ip;
    }

    private async Task<TResponse?> GetResponseOrDefault<TResponse>(
        HttpClient ipifyOrgClient, string requestUri, CancellationToken token) where TResponse : class
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

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy<HttpResponseMessage>
        .Handle<Exception>()
        .OrResult(r => r.IsSuccessStatusCode is false)
        .RetryAsync(10);

    private record IpifyGetIpResponse(string Ip);

    private record IpApiComGetIpResponse([property: JsonPropertyName("query")] string Ip, bool Proxy);

    public record IpFallBack(string Ip);
}