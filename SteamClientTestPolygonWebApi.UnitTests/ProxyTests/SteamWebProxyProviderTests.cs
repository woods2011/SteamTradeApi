using FluentAssertions;
using Microsoft.Extensions.Options;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure;

namespace SteamClientTestPolygonWebApi.Tests.ProxyTests;

public class SteamWebProxyProviderTests
{
    [Fact]
    public void GetProxy_ShouldReturnProxies_WhenFileContainsThemInStandardFormat()
    {
        //Arrange
        var initialProxyPool = new Uri[]
        {
            new("socks5://72.195.114.184:4145"),
            new("socks5://184.178.172.26:4145"),
            new("socks5://104.37.135.145:4145"),
            new("socks5://167.235.28.249:4000"),
            new("socks5://72.217.216.239:4145"),
            new("socks5://72.206.181.123:4145"),
            new("socks5://184.170.245.148:4145"),
            new("socks5://98.162.25.7:31653"),
            new("socks5://72.206.181.97:64943"),
            new("socks5://142.54.235.9:4145")
        };

        var batchSize = 4;
        var requestsPerProxy = 3;

        var sut = new PooledWebProxyProvider(initialProxyPool, Options.Create(new ProxyPoolSettings
        {
            BatchSize = batchSize,
            RequestsPerProxy = requestsPerProxy
        }));

        //Act
        var proxyList = Enumerable.Range(0, initialProxyPool.Length * requestsPerProxy)
            .Select(_ => sut.GetProxy())
            .ToList();

        //Assert
        proxyList.Should().NotBeEmpty();

        var shouldRespectNumOfAppearances = RepeatCollection(initialProxyPool, requestsPerProxy);
        proxyList.Should().Contain(shouldRespectNumOfAppearances);

        var shouldRespectBatching = RepeatCollection(initialProxyPool.Take(batchSize), requestsPerProxy);
        proxyList.Should().ContainInConsecutiveOrder(shouldRespectBatching);


        IEnumerable<Uri> RepeatCollection(IEnumerable<Uri> uris, int times) =>
            Enumerable.Range(0, times)
                .Aggregate(Enumerable.Empty<Uri>(), (acc, _) => acc.Concat(uris)).ToList();
    }
}