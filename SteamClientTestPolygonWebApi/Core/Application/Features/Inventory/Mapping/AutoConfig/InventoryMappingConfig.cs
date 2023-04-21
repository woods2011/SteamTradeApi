using Mapster;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Core.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Core.Domain.Item;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Mapping.AutoConfig;

public class InventoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GameInventory, GameInventoryFullProjection>()
            .MapToConstructor(true);

        config.NewConfig<GameItem, GameItemFullProjection>()
            .MapToConstructor(true);
        
        config.NewConfig<GameInventory, GameInventorySplitProjection>()
            .MapToConstructor(true);

        config.NewConfig<GameInventory, GameInventoryTradabilityProjection>()
            .MapToConstructor(true);

        config.NewConfig<GameItem, GameItemStackPriceProjection>()
            .MapToConstructor(true);
    }
}