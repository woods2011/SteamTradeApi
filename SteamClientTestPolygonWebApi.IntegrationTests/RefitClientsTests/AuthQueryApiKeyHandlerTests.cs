using System.Text;
using AutoFixture.Xunit2;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Helpers.Refit;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

namespace SteamClientTestPolygonWebApi.IntegrationTests.RefitClientsTests;

public class AuthQueryApiKeyHandlerTests
{
    [Theory, AutoData]
    public async Task SendAsync_ShouldInsertApiKeyInRequestQuery(int pingMs, int time, int works, string apiKey)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp
            .When("https://api.good-proxies.ru/get.php")
            .Respond(request => new StringContent(request.RequestUri?.ToString() ?? "No Uri", Encoding.UTF8));

        var httpClient = new HttpClient(AuthQueryApiKeyHandler.CreateInstance(mockHttp, apiKey))
            { BaseAddress = new Uri("https://api.good-proxies.ru") };
        var api = RestService.For<IGoodProxiesRuApi>(httpClient);

        // Act
        var result = await api.GetProxies(SupportedProxiesSchemes.Http, pingMs, time, works);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrWhiteSpace();
        new Uri(result.Content!).Query.Should().Contain(apiKey);
    }
}