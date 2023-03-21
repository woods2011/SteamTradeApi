using System.Net;
using AutoFixture;
using FluentAssertions;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

public class Load : IClassFixture<InventoryWebAppFactory>
{
    private readonly InventoryWebAppFactory _factory;
    private readonly Fixture _fixture;

    public Load(InventoryWebAppFactory factory)
    {
        _factory = factory;
        _fixture = factory.Fixture;
    }


    [Fact]
    public async Task Load_ReturnsCreated_WhenInventoryLoadedFirstTime()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        var firstRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);

        //Act
        var loadCreatedResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        loadCreatedResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.MockHttp.GetMatchCount(firstRequest).Should().Be(1);
    }

    
    [Fact]
    public async Task Load_ReturnsNoContent_WhenInventoryExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        var request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);
        var createdResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Act
        var loadNoContentResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        loadNoContentResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.MockHttp.GetMatchCount(request).Should().Be(2);
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task Load_ReturnsNotFound_WhenSteamInventoryIsNotFoundOrHided()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var notFoundRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.NotFound);
        var forbiddenRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.Forbidden);
        var nullJsonResponse = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", "null");

        //Act
        var whenNotFoundResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        var whenForbiddenResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        var whenNullJsonResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        whenNotFoundResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        whenForbiddenResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        whenNullJsonResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _factory.MockHttp.GetMatchCount(notFoundRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(forbiddenRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(nullJsonResponse).Should().Be(1);
    }


    [Fact]
    public async Task Load_ReturnsBadGateway_WhenSteamContinueToReturn429() // ToDo
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var totalRetryCountPlusOne = 4;
        var request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.TooManyRequests);

        //Act
        var response = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        _factory.MockHttp.GetMatchCount(request).Should().Be(totalRetryCountPlusOne);
    }
}