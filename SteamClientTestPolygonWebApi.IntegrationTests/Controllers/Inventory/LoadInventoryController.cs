using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

[TestCaseOrderer("SteamClientTestPolygonWebApi.IntegrationTests.Helpers.AlphabeticalOrderer",
    "SteamClientTestPolygonWebApi.IntegrationTests")] // Is this a bad practice at this case?
public class LoadInventoryController : IClassFixture<InventoryControllerWebApplicationFactory>
{
    private readonly InventoryControllerWebApplicationFactory _factory;
    private readonly Fixture _fixture;

    public LoadInventoryController(InventoryControllerWebApplicationFactory factory)
    {
        _factory = factory;
        _fixture = factory.Fixture;
    }


    [Fact]
    public async Task Post_ReturnsBadGateway_WhenSteamContinueToReturn429() // ToDo
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var totalRetryCountPlusOne = 4;
        var request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond(HttpStatusCode.TooManyRequests);

        //Act
        var act = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        (await act()).StatusCode.Should().Be(HttpStatusCode.BadGateway);
        _factory.MockHttp.GetMatchCount(request).Should().Be(totalRetryCountPlusOne);
    }

    [Fact]
    public async Task Post_ReturnsNotFound_WhenSteamInventoryIsNotFoundOrHided()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var notFoundRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond(HttpStatusCode.NotFound);
        var forbiddenRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond(HttpStatusCode.Forbidden);
        var nullJsonResponse = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond("application/json", "null");

        //Act
        var actNotFoundRequest = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        var actForbiddenRequest = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        var actNullJsonResponse = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        (await actNotFoundRequest()).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await actForbiddenRequest()).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await actNullJsonResponse()).StatusCode.Should().Be(HttpStatusCode.NotFound);
        _factory.MockHttp.GetMatchCount(notFoundRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(forbiddenRequest).Should().Be(1);
        _factory.MockHttp.GetMatchCount(nullJsonResponse).Should().Be(1);
    }

    [Fact]
    public async Task B_Post_ReturnsNoContent_WhenInventoryExists()
    {
        //Arrange
        var (steam64Id, appId) = (76561198000000000L, 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        var secondRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond("application/json", serializedSteamSdkInventory);

        //Act
        var actLoadUpdate = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        var loadNoContentResponse = await actLoadUpdate();
        loadNoContentResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.MockHttp.GetMatchCount(secondRequest).Should().Be(1);
    }

    [Fact]
    public async Task A_Post_ReturnsCreated_WhenInventoryLoadedFirstTime()
    {
        //Arrange
        var (steam64Id, appId) = (76561198000000000L, 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        var firstRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond("application/json", serializedSteamSdkInventory);

        //Act
        var actLoadInsert = () => _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Assert
        var loadCreatedResponse = await actLoadInsert();
        loadCreatedResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        _factory.MockHttp.GetMatchCount(firstRequest).Should().Be(1);
    }
    
    // await using var dbContextFactory = _factory.CreateDbContextFactory();
    // var dbContext = dbContextFactory.DbCtx;
}