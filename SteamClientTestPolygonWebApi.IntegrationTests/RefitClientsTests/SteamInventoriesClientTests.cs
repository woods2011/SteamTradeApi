using System.Net;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Refit;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.IntegrationTests.RefitClientsTests;

public class SteamInventoriesClientTests
{
    private readonly Fixture _fixture = new();

    [Theory, AutoData]
    public async Task GetInventory_ShouldSendRequestOnCorrectUri(
        long steam64Id,
        int appId,
        string startAssetId,
        int maxCount)
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var expectedUri = $"https://steamcommunity.com/inventory/" +
                          $"{steam64Id}/{appId}/2?l=english&start_assetid={startAssetId}&count={maxCount}";

        var request = mockHttp.Expect(expectedUri).Respond(HttpStatusCode.OK);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://steamcommunity.com");
        var api = RestService.For<IOfficialSteamInventoriesClient>(httpClient);

        // Act
        var result = await api.GetInventory(steam64Id, appId, startAssetId, maxCount);

        // Assert
        mockHttp.GetMatchCount(request).Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetInventory_ShouldReturnProperDeserializedInventory()
    {
        // Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);
        var mockHttp = new MockHttpMessageHandler();

        var serializedInventoryResponse = await File.ReadAllTextAsync(
            $"{Directory.GetCurrentDirectory()}/ExternalApisResponsesJson/SteamSdkInventoryResponseExample.json");
        var request = mockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedInventoryResponse);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://steamcommunity.com");
        var api = RestService.For<IOfficialSteamInventoriesClient>(httpClient,
            new RefitSettings(new SystemTextJsonContentSerializer(SteamApiJsonSettings.Default)));

        // Act
        var result = await api.GetInventory(steam64Id, appId);

        // Assert
        mockHttp.GetMatchCount(request).Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Content.Should().NotBeNull();
    }
}