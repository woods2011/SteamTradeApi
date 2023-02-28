namespace SteamClientTestPolygonWebApi.Contracts.Responses;

public record SteamInventoryResponse(IEnumerable<ItemWithDescriptionResponse> Items);

public record ItemWithDescriptionResponse(string AssetId, int Tradable);