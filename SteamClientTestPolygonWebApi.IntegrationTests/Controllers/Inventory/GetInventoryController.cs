using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

public class GetInventoryController : IClassFixture<InventoryControllerWebApplicationFactory>
{
    private readonly InventoryControllerWebApplicationFactory _factory;
    private readonly Fixture _fixture;

    public GetInventoryController(InventoryControllerWebApplicationFactory factory)
    {
        _factory = factory;
        _fixture = factory.Fixture;
    }


    [Fact]
    public async Task Get_ReturnsOkWithNotEmptyResult_WhenInventoryIsExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2?count=5000")
            .Respond("application/json", serializedSteamSdkInventory);
        await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Act
        var act = () => _factory.Client.GetAsync($"/Inventory/{steam64Id}/{appId}");

        //Assert
        var response = await act();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var inventoryProjection = await response.Content.ReadFromJsonAsync<GameInventoryFullProjection>();
        inventoryProjection.Should().NotBeNull();
        inventoryProjection!.AppId.Should().Be(appId);
        inventoryProjection!.OwnerSteam64Id.Should().Be(steam64Id.ToString());
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenInventoryNotExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        //Act
        var act = () => _factory.Client.GetAsync($"/Inventory/{steam64Id}/{appId}");

        //Assert
        (await act()).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}