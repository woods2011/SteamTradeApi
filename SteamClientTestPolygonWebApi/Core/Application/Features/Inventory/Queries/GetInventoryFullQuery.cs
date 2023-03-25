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
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }
}

public class GetSteamInventoryFullQueryHandler :
    IRequestHandler<GetInventoryFullQuery, OneOf<GameInventoryFullProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetSteamInventoryFullQueryHandler> _logger;

    public GetSteamInventoryFullQueryHandler(
        SteamTradeApiDbContext dbCtx,
        IMapper mapper,
        IDistributedCache cache,
        ILogger<GetSteamInventoryFullQueryHandler> logger)
    {
        _dbCtx = dbCtx;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<OneOf<GameInventoryFullProjection, NotFound>> Handle(GetInventoryFullQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);
        var entryKey = $"InventoryMainProjection-{query.Steam64Id}{query.AppId}";
        
        var inventorySerialized = await _cache.GetStringAsync(entryKey, token);
        var inventoryMainProjection = inventorySerialized is not null
            ? JsonSerializer.Deserialize<GameInventoryFullProjection>(inventorySerialized)
            : null;

        if (inventoryMainProjection is not null) return inventoryMainProjection;

        inventoryMainProjection = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .ProjectToType<GameInventoryFullProjection>()
            .FirstOrDefaultAsync(token);
        
        if (inventoryMainProjection is null) return new NotFound();

        var cacheOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
        var serializedInventory = JsonSerializer.Serialize(inventoryMainProjection);
        await _cache.SetStringAsync(entryKey, serializedInventory, cacheOptions, token);

        return inventoryMainProjection;
    }
}