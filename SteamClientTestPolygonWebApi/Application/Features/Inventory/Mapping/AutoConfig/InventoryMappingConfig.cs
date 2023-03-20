using Mapster;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Domain.Item;

namespace SteamClientTestPolygonWebApi.Application.Features.Inventory.Mapping.AutoConfig;

public class InventoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GameInventory, GameInventoryFullProjection>()
            .MapToConstructor(true);

        config.NewConfig<GameItem, GameItemFullProjection>().MapToConstructor(true);;
    }
}
