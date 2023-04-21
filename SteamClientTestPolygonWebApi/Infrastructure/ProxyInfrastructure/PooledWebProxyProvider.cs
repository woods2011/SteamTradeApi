using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using Microsoft.Extensions.Options;
using SteamClientTestPolygonWebApi.Helpers.Extensions;

namespace SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure;

public interface IProxyUpdateConsumer
{
    void RefreshProxyPool(IEnumerable<Uri> newProxyPool);
}

public class PooledWebProxyProvider : IWebProxy, IProxyUpdateConsumer
{
    private int _proxyIndex = 0;
    private readonly int _batchSize;
    private readonly int _requestsPerProxy;

    public ReadOnlyCollection<Uri> ProxyPool { get; private set; }


    public PooledWebProxyProvider(IOptions<ProxyPoolSettings> settings) : this(Array.Empty<Uri>(), settings) { }

    internal PooledWebProxyProvider(IEnumerable<Uri> proxyPool, IOptions<ProxyPoolSettings> settings)
    {
        _ = settings ?? throw new ArgumentNullException(nameof(settings));
        _batchSize = settings.Value.BatchSize;
        _requestsPerProxy = settings.Value.RequestsPerProxy;
        ProxyPool = proxyPool.ToList().AsReadOnly();
    }

    internal PooledWebProxyProvider(IReadOnlyDictionary<Uri, NetworkCredential> proxyPoolWithCredentials,
        IOptions<ProxyPoolSettings> settings) : this(settings)
    {
        var credentials = new CredentialCache();
        foreach (var (proxy, credential) in proxyPoolWithCredentials)
            credentials.Add(proxy, proxy.Scheme, credential);

        Credentials = credentials;
        ProxyPool = proxyPoolWithCredentials.Keys.ToList().AsReadOnly();
    }

    public static PooledWebProxyProvider CreateWithCredentials(
        IReadOnlyDictionary<Uri, NetworkCredential> proxyPoolWithCredentials, IOptions<ProxyPoolSettings> settings)
        => new(proxyPoolWithCredentials, settings);

    public static PooledWebProxyProvider CreateWithCredentials(
        IReadOnlyDictionary<Uri, NetworkCredential> proxyPoolWithCredentials, int batchSize, int requestsPerProxy)
        => new(proxyPoolWithCredentials,
            Options.Create(new ProxyPoolSettings { BatchSize = batchSize, RequestsPerProxy = requestsPerProxy }));

    public bool IsBypassed(Uri host) => false;

    public Uri GetProxy(Uri? _ = null)
    {
        var proxyPool = ProxyPool;
        
        if (proxyPool.Count == 0)
            return new Uri("socks5://1.1.1.1"); // ToDo: temporary solution
            //throw new LackOfProxiesException("Proxy pool is empty");

        var proxyIndex = Interlocked.Increment(ref _proxyIndex) - 1;
        var batchIndex = proxyIndex / (_requestsPerProxy * _batchSize);
        var inBatchIndex = proxyIndex % _batchSize;
        var realProxyIndex = (batchIndex * _batchSize + inBatchIndex) % proxyPool.Count;
        return proxyPool[realProxyIndex];
    }

    public void RefreshProxyPool(IEnumerable<Uri> newProxyPool)
    {
        newProxyPool = newProxyPool.MaterializeToIReadOnlyCollection();

        var (currentProxyPool, proxyIndex) = (ProxyPool, _proxyIndex);
        if (currentProxyPool.Count is 0)
        {
            ProxyPool = newProxyPool.ToList().AsReadOnly();
            return;
        }

        var batchIndex = proxyIndex / (_requestsPerProxy * _batchSize);
        var mostFreshIndex = (batchIndex + 1) * _batchSize % currentProxyPool.Count;

        var leastFreshProxies = currentProxyPool.Take(mostFreshIndex);
        var oldProxiesOrderByFreshness = currentProxyPool.Skip(mostFreshIndex).Concat(leastFreshProxies);
        var validOldProxies = oldProxiesOrderByFreshness.Intersect(newProxyPool).ToList();

        _proxyIndex = 0;
        ProxyPool = newProxyPool.Except(validOldProxies).Concat(validOldProxies).ToList().AsReadOnly();
    }

    public void RefreshProxyPool(IEnumerable<Uri> newProxyPool, IEnumerable<Uri> stillValidOldProxies)
    {
        var (currentProxyPool, proxyIndex) = (ProxyPool, _proxyIndex);
        if (currentProxyPool.Count is 0)
        {
            ProxyPool = newProxyPool.ToList().AsReadOnly();
            return;
        }

        var batchIndex = proxyIndex / (_requestsPerProxy * _batchSize);
        var mostFreshIndex = (batchIndex + 1) * _batchSize % currentProxyPool.Count;

        var leastFreshProxies = currentProxyPool.Take(mostFreshIndex);
        var oldProxiesOrderByFreshness = currentProxyPool.Skip(mostFreshIndex).Concat(leastFreshProxies);

        _proxyIndex = 0;
        ProxyPool = newProxyPool.Concat(oldProxiesOrderByFreshness.Intersect(stillValidOldProxies))
            .ToList().AsReadOnly();
    }


    public ICredentials? Credentials { get; set; }
}

// ToDO: Add validation
public class ProxyPoolSettings
{
    public int BatchSize { get; init; }
    public int RequestsPerProxy { get; init; }
}

public class LackOfProxiesException : Exception
{
    public LackOfProxiesException() { }

    protected LackOfProxiesException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public LackOfProxiesException(string? message) : base(message) { }

    public LackOfProxiesException(string? message, Exception? innerException) : base(message, innerException) { }
}