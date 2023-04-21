using System.Net;
using AutoFixture.Xunit2;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Helpers.Refit;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker;
using SteamClientTestPolygonWebApi.Infrastructure.ProxyInfrastructure.Checker.ProxySources.GoodProxiesRu;

namespace SteamClientTestPolygonWebApi.IntegrationTests.RefitClientsTests;

public class GoodProxiesApiTests
{
    [Theory, AutoData]
    public async Task GetProxies_ShouldSendRequestOnCorrectUri(int pingMs, int time, int works, string apiKey)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var expectedUri =
            $"https://api.good-proxies.ru/get.php?type[http]=on&ping={pingMs}&time={time}&works={works}&key={apiKey}";

        var request = mockHttp.Expect(expectedUri).Respond(HttpStatusCode.OK);

        var httpClient = new HttpClient(AuthQueryApiKeyHandler.CreateInstance(mockHttp, apiKey))
            { BaseAddress = new Uri("https://api.good-proxies.ru") };
        var api = RestService.For<IGoodProxiesRuApi>(httpClient);

        // Act
        ApiResponse<string> result =
            await api.GetProxies(type: SupportedProxiesSchemes.Http, pingMs: pingMs, time: time, works: works);

        // Assert
        mockHttp.GetMatchCount(request).Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}