namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

[Collection(nameof(InventoryWebAppFactoryCollection))]
public class GetInventoryController
{
    private readonly GeneralWebApplicationFactory _webApplicationFactory;

    public GetInventoryController(GeneralWebApplicationFactory webApplicationFactory) =>
        _webApplicationFactory = webApplicationFactory;
}