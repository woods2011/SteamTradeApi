namespace SteamClientTestPolygonWebApi.IntegrationTests.Controllers.Inventory;

[Collection(nameof(InventoryWebAppFactoryCollection))]
public class LoadInventoryController
{
    private readonly GeneralWebApplicationFactory _webApplicationFactory;

    public LoadInventoryController(GeneralWebApplicationFactory webApplicationFactory) =>
        _webApplicationFactory = webApplicationFactory;
}