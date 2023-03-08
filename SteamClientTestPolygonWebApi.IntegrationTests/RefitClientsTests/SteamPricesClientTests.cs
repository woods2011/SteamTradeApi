using System.Net;
using AutoFixture.Xunit2;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

namespace SteamClientTestPolygonWebApi.IntegrationTests.RefitClientsTests;

public class SteamPricesClientTests
{
    [Theory, AutoData]
    public async Task GetInventory_ShouldSendRequestOnCorrectUri(int appId, string marketHashName)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUri =
            $"https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid={appId}&market_hash_name={marketHashName}";

        var request = mockHttp.Expect(expectedUri).Respond(HttpStatusCode.OK);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://steamcommunity.com");
        var api = RestService.For<ISteamPricesClient>(httpClient);

        // Act
        var result = await api.GetItemLowestMarketPrice(appId, marketHashName);

        // Assert
        mockHttp.GetMatchCount(request).Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}