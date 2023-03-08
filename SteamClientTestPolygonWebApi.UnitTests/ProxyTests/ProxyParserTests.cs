using FluentAssertions;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;

namespace SteamClientTestPolygonWebApi.Tests.ProxyTests;

public class ProxyParserTests
{
    //generate tests for proxyparser

    [InlineData(SupportedProxiesSchemes.Http)]
    [InlineData(SupportedProxiesSchemes.Socks4)]
    [InlineData(SupportedProxiesSchemes.Socks5)]
    [Theory]
    public void Parse_ShouldReturnUri_WhenLineContainsItInRightFormatWithScheme(string proxyType)
    {
        //Arrange
        var proxyStr = $"{proxyType}://155.94.178.38:16666";
        
        //Act
        var uri = ProxyParser.Parse(proxyStr);
        
        //Assert
        uri.Scheme.Should().BeEquivalentTo(proxyType);
        uri.ToString().Should().ContainEquivalentOf(proxyStr);
    }
    
    [InlineData(SupportedProxiesSchemes.Http)]
    [InlineData(SupportedProxiesSchemes.Socks4)]
    [InlineData(SupportedProxiesSchemes.Socks5)]
    [Theory]
    public void Parse_ShouldReturnUri_WhenLineContainsItInRightFormatWithoutScheme(string proxyType)
    {
        //Arrange
        var proxyStr = "155.94.178.38:16666";
        
        //Act
        var uri = ProxyParser.Parse(proxyStr, proxyType);
        
        //Assert
        uri.Scheme.Should().BeEquivalentTo(proxyType);
        uri.ToString().Should().ContainEquivalentOf(proxyStr);
    }
    
    [Fact]
    public void Parse_ShouldThrowArgumentException_WhenSchemeNotSpecified()
    {
        //Arrange
        var proxyStr = "155.94.178.38:16666";
    
        //Act
        var action = () => ProxyParser.Parse(proxyStr);
        
        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Uri is in the wrong format");
    }
    
    [InlineData("http", "socks4")]
    [InlineData("http", "socks5")]
    [InlineData("socks4", "http")]
    [InlineData("socks4", "socks5")]
    [InlineData("socks5", "http")]
    [InlineData("socks5", "socks4")]
    [Theory]
    public void Parse_ShouldThrowArgumentException_WhenSchemesAreDifferent(string implicitScheme, string explicitScheme)
    {
        //Arrange
        var proxyStr = $"{implicitScheme}{Uri.SchemeDelimiter}155.94.178.38:16666";
    
        //Act
        var act = () => ProxyParser.Parse(proxyStr, explicitScheme);
        
        //Assert
        var argumentException = act.Should().Throw<ArgumentException>().Which;
        argumentException.Message.Should().ContainAll(implicitScheme, explicitScheme);
    }
    
    
    [InlineData("http")]
    [InlineData("socks4")]
    [InlineData("socks5")]
    [Theory]
    public void Parse_ShouldReturnUri_WhenSchemesAreIdentical(string scheme)
    {
        //Arrange
        var proxyStr = $"{scheme}{Uri.SchemeDelimiter}155.94.178.38:16666";
    
        //Act
        var uri = ProxyParser.Parse(proxyStr, scheme);
        
        //Assert
        uri.Scheme.Should().BeEquivalentTo(scheme);
        uri.ToString().Should().ContainEquivalentOf(proxyStr);
    }
}