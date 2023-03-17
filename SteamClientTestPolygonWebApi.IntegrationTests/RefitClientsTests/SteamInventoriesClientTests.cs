using System.Net;
using AutoFixture.Xunit2;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

namespace SteamClientTestPolygonWebApi.IntegrationTests.RefitClientsTests;

public class SteamInventoriesClientTests
{
    [Theory, AutoData]
    public async Task GetInventory_ShouldSendRequestOnCorrectUri(long steamId, int appId, int maxCount)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUri = $"https://steamcommunity.com/inventory/{steamId}/{appId}/2?l=english&count={maxCount}";

        var request = mockHttp.Expect(expectedUri).Respond(HttpStatusCode.OK);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://steamcommunity.com");
        var api = RestService.For<ISteamInventoriesClient>(httpClient);

        // Act
        var result = await api.GetInventoryInternal(steamId, appId, maxCount);

        // Assert
        mockHttp.GetMatchCount(request).Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}