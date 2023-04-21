using System.Net;
using AutoFixture;
using FluentAssertions;
using RichardSzalay.MockHttp;
using SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory.Setup;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

public class LoadPrices : IClassFixture<InventoryWebAppFactory>
{
    private readonly InventoryWebAppFactory _factory;
    private readonly Fixture _fixture;

    public LoadPrices(InventoryWebAppFactory factory)
    {
        _factory = factory;
        _fixture = factory.Fixture;
    }
    
    
    [Fact]
    public async Task LoadPrices_ReturnsNoContent_WhenPricesLoaded()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        var serializedSteamSdkInventory = _factory.SerializedSteamSdkInventoryResponseExample;
        MockedRequest firstRequest = _factory.MockHttp
            .Expect(HttpMethod.Get, $"https://steamcommunity.com/inventory/{steam64Id}/{appId}/2*")
            .Respond("application/json", serializedSteamSdkInventory);
        HttpResponseMessage loadCreatedResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}", null);
        
        var serializedSteamSdkItemPrice = _factory.SerializedSteamSdkItemPriceResponseExample;
        MockedRequest request = _factory.MockHttp
            .When(HttpMethod.Get, $"https://steamcommunity.com/market/priceoverview/?country=us&currency=1&appid={appId}")
            .Respond("application/json", serializedSteamSdkItemPrice);

        //Act
        HttpResponseMessage loadNoContentResponse = await _factory.Client.PostAsync($"/Inventory/{steam64Id}/{appId}/Prices", null);

        //Assert
        loadNoContentResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.MockHttp.GetMatchCount(firstRequest).Should().Be(1);
    }
    
    [Fact]
    public async Task LoadPrices_ReturnsNotFound_WhenInventoryNotExists()
    {
        //Arrange
        var (steam64Id, appId) = (Math.Abs(_fixture.Create<long>()), 730);

        //Act
        HttpResponseMessage response = await _factory.Client.GetAsync($"/Inventory/{steam64Id}/{appId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}