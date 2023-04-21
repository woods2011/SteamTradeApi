using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Queries;

public class GetInventoryFullQuery : IRequest<OneOf<GameInventoryFullProjection, NotFound>>
{
    [Required]
    public long Steam64Id { get; init; }
    
    [Required]
    public int AppId { get; init; }
}

public class GetSteamInventoryFullQueryHandler :
    IRequestHandler<GetInventoryFullQuery, OneOf<GameInventoryFullProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly IDistributedCache _cache;

    public GetSteamInventoryFullQueryHandler(SteamTradeApiDbContext dbCtx, IDistributedCache cache)
    {
        _dbCtx = dbCtx;
        _cache = cache;
    }

    public async Task<OneOf<GameInventoryFullProjection, NotFound>> Handle(GetInventoryFullQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);
        var entryKey = $"InventoryMainProjection-{steam64Id}{appId}";

        GameInventoryFullProjection? inventoryMainProjection = await GetFromCacheOrDefault();
        if (inventoryMainProjection is not null) return inventoryMainProjection;

        inventoryMainProjection = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .ProjectToType<GameInventoryFullProjection>()
            .FirstOrDefaultAsync(token);
        
        if (inventoryMainProjection is null) return new NotFound();

        await SetCache();

        return inventoryMainProjection;
        

        async Task<GameInventoryFullProjection?> GetFromCacheOrDefault()
        {
            var inventorySerialized = await _cache.GetStringAsync(entryKey, token);
            GameInventoryFullProjection? cachedResult = inventorySerialized is not null
                ? JsonSerializer.Deserialize<GameInventoryFullProjection>(inventorySerialized)
                : null;
            return cachedResult;
        }

        async Task SetCache()
        {
            var cacheOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
            var serializedInventory = JsonSerializer.Serialize(inventoryMainProjection);
            await _cache.SetStringAsync(entryKey, serializedInventory, cacheOptions, token);
        }
    }
}