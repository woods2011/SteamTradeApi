using FluentAssertions;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;

namespace SteamClientTestPolygonWebApi.UnitTests.ProxyTests;

public class ProxyParserTests
{
    [Theory]
    [MemberData(nameof(AllSupportedProxiesSchemesData))]
    public void Parse_ShouldReturnUri_WhenLineContainsItInRightFormatWithScheme(string proxyScheme)
    {
        //Arrange
        var proxyStr = $"{proxyScheme}://155.94.178.38:16666";

        //Act
        var uri = ProxyParser.Parse(proxyStr);

        //Assert
        uri.Scheme.Should().BeEquivalentTo(proxyScheme);
        uri.ToString().Should().ContainEquivalentOf(proxyStr);
    }


    [Theory]
    [MemberData(nameof(AllSupportedProxiesSchemesData))]
    public void Parse_ShouldReturnUri_WhenLineContainsItInRightFormatWithoutScheme(string proxyScheme)
    {
        //Arrange
        var proxyStr = "155.94.178.38:16666";

        //Act
        var uri = ProxyParser.Parse(proxyStr, proxyScheme);

        //Assert
        uri.Scheme.Should().BeEquivalentTo(proxyScheme);
        uri.ToString().Should().ContainEquivalentOf(proxyStr);
    }


    [Theory]
    [MemberData(nameof(AllSupportedProxiesSchemesData))]
    public void Parse_ShouldReturnUri_WhenSchemesAreIdentical(string scheme)
    {
        //Arrange
        var proxyStr = $"{scheme}://155.94.178.38:16666";

        //Act
        var uri = ProxyParser.Parse(proxyStr, scheme);

        //Assert
        uri.Scheme.Should().BeEquivalentTo(scheme);
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


    [Theory]
    [MemberData(nameof(DifferentProxiesSchemesPairs))]
    public void Parse_ShouldThrowArgumentException_WhenSchemesAreDifferent(string implicitScheme, string explicitScheme)
    {
        //Arrange
        var proxyStr = $"{implicitScheme}://155.94.178.38:16666";

        //Act
        var act = () => ProxyParser.Parse(proxyStr, explicitScheme);

        //Assert
        var argumentException = act.Should().Throw<ArgumentException>().Which;
        argumentException.Message.Should().ContainAll(implicitScheme, explicitScheme);
    }


    public static IEnumerable<object[]> AllSupportedProxiesSchemesData =>
        SupportedProxiesSchemes.All.Select(scheme => new object[] { scheme });

    public static IEnumerable<object[]> DifferentProxiesSchemesPairs =>
        from scheme1 in SupportedProxiesSchemes.All
        from scheme2 in SupportedProxiesSchemes.All
        where scheme1 != scheme2
        select new object[] { scheme1, scheme2 };
}