﻿using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FluentAssertions;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

public class GetTradabilityProjection : IClassFixture<InventoryWebAppFactory>
{
    private readonly InventoryWebAppFactory _factory;
    private readonly Fixture _fixture;

    public GetTradabilityProjection(InventoryWebAppFactory factory)
    {
        _factory = factory;
        _fixture = factory.Fixture;
    }


    [Fact]
    public async Task GetTradabilityProjection_ReturnsOkWithNotEmptyResult_WhenInventoryExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);
        await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);

        //Act
        HttpResponseMessage response = await _factory.Client.GetAsync($"/Inventory/{steam64Id}/{appId}/Tradability");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var inventoryProjection = await response.Content.ReadFromJsonAsync<GameInventoryTradabilityProjection>();
        inventoryProjection.Should().NotBeNull();
        
        inventoryProjection!.AppId.Should().Be(appId);
        inventoryProjection.OwnerSteam64Id.Should().Be(steam64Id.ToString());
        
        inventoryProjection.TotalAssetsCount.Should().Be(inventoryProjection.Assets.Count);
        inventoryProjection.TotalAssetsCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetTradabilityProjection_ReturnsNotFound_WhenInventoryNotExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        //Act
        HttpResponseMessage response = await _factory.Client.GetAsync($"/Inventory/{steam64Id}/{appId}/Tradability");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}