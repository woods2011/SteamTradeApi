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
        MockedRequest request = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);

        //Act
        HttpResponseMessage loadCreatedResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        loadCreatedResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.MockHttp.GetMatchCount(request).Should().Be(1);
    }

    
    [Fact]
    public async Task Load_ReturnsNoContent_WhenInventoryExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        MockedRequest request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);
        HttpResponseMessage createdResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Act
        HttpResponseMessage loadNoContentResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

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

        MockedRequest notFoundRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.NotFound);
        MockedRequest forbiddenRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.Forbidden);
        MockedRequest nullJsonResponse = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", "null");

        //Act
        HttpResponseMessage whenNotFoundResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        HttpResponseMessage whenForbiddenResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        HttpResponseMessage whenNullJsonResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        whenNotFoundResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        whenForbiddenResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        whenNullJsonResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _factory.MockHttp.GetMatchCount(notFoundRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(forbiddenRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(nullJsonResponse).Should().Be(1);
    }


    [Fact]
    public async Task Load_ReturnsGatewayTimeout_WhenSteamReturns429() // ToDo
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        MockedRequest request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond(HttpStatusCode.TooManyRequests);

        //Act
        HttpResponseMessage response = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.GatewayTimeout);
        _factory.MockHttp.GetMatchCount(request).Should().Be(1);
    }
}