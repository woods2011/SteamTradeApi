using System.Net;
using Polly;
using Polly.Retry;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxyAnonymityJudges;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;

public class ProxyChecker
{
    private readonly IProxyAnonymityJudge _proxyAnonymityJudge;

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy<HttpResponseMessage>
        .Handle<Exception>()
        .OrResult(r => r.IsSuccessStatusCode is false)
        .RetryAsync(3);


    public ProxyChecker(IProxyAnonymityJudge proxyAnonymityJudge) =>
        _proxyAnonymityJudge = proxyAnonymityJudge;


    /// <summary>
    /// Returns the result of proxy checking or null if proxy is dead
    /// </summary>
    /// <param name="proxyUri">An Uri of proxy server to check</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>null if proxy is dead; otherwise, result of proxy check</returns>
    public async Task<ProxyCheckResult?> CheckProxyAsync(Uri proxyUri, CancellationToken token)
    {
        var httpMessageHandler = new HttpClientHandler { Proxy = new WebProxy(proxyUri) };
        var client = new HttpClient(httpMessageHandler) { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.ConnectionClose = true;

        var totalRetryCount = -1;
        var executeResult = await _retryPolicy.ExecuteAndCaptureAsync(ct =>
        {
            totalRetryCount++;
            return client.GetAsync(_proxyAnonymityJudge.ContentUri, ct);
        }, token);

        if (executeResult.Outcome is OutcomeType.Failure)
            return null;

        var anonymityLevel = await _proxyAnonymityJudge.Judge(executeResult.Result.Content);

        return new ProxyCheckResult(anonymityLevel, totalRetryCount);
        // ToDo: extract availability check to separate class
    }

    // .RetryAsync(3, onRetry: (_, retryCount, context) => context["RetriesInvoked"] = retryCount);
    // var retryCount = executeResult.Context?.GetValueOrDefault("RetriesInvoked") ?? 0;
}

public record ProxyCheckResult(ProxyAnonymityLevel ProxyAnonymityLevel, int RetryCount);