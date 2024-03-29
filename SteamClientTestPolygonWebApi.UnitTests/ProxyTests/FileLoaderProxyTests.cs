﻿using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources;

namespace SteamClientTestPolygonWebApi.UnitTests.ProxyTests;

public class FileLoaderProxyTests
{
    [Fact]
    public async void PopulateProxyPool_ShouldReturnUris_WhenFileContainsItInRightFormat()
    {
        //Arrange
        const string scheme = SupportedProxiesSchemes.Socks5;

        var lines = new[]
        {
            "72.195.114.184:4145",
            "184.178.172.26:4145",
            "104.37.135.145:4145",
            "167.235.28.249:4000",
            "72.217.216.239:4145",
            "72.206.181.123:4145",
            "184.170.245.148:4145",
            "98.162.25.7:31653",
            "72.206.181.97:64943",
            "142.54.235.9:4145"
        };

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile($"Files/ProxyPool_{scheme}.txt", new MockFileData(
            String.Join(Environment.NewLine, lines)));

        var sut = new FileProxySource(mockFileSystem) { Scheme = scheme };

        //Act
        List<Uri> proxyPool = (await sut.GetProxiesAsync(CancellationToken.None)).ToList();

        //Assert
        proxyPool.Should().AllSatisfy(uri => uri.Scheme.Should().BeEquivalentTo(scheme));
        proxyPool.Should().HaveSameCount(lines);
        for (var i = 0; i < lines.Length; i++) proxyPool[i].ToString().Should().Contain(lines[i]);
    }
}