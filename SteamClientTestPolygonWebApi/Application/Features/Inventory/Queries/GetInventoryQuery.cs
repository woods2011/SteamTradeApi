using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Application.Features.Inventory.Queries;

public class GetSteamInventoryQuery : IRequest<OneOf<GameInventoryMainProjection, NotFound>>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }
}

public class GetSteamInventoryQueryHandler
    : IRequestHandler<GetSteamInventoryQuery, OneOf<GameInventoryMainProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetSteamInventoryQueryHandler> _logger;

    public GetSteamInventoryQueryHandler(
        SteamTradeApiDbContext dbCtx,
        IMapper mapper,
        IDistributedCache cache,
        ILogger<GetSteamInventoryQueryHandler> logger)
    {
        _dbCtx = dbCtx;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<OneOf<GameInventoryMainProjection, NotFound>> Handle(GetSteamInventoryQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);

        var entryKey = $"InventoryMainProjection-{query.Steam64Id}{query.AppId}";
        var inventorySerialized = await _cache.GetStringAsync(entryKey, token);

        GameInventoryMainProjection? inventoryMainProjection = null;
        if (inventorySerialized != null)
            inventoryMainProjection = JsonSerializer.Deserialize<GameInventoryMainProjection>(inventorySerialized);
        if (inventoryMainProjection != null) return inventoryMainProjection;

        // change to .ProjectToType<GameInventoryMainProjection>()
        var inventory = await _dbCtx.Inventories
            .AsNoTracking()
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .FirstOrDefaultAsync(token);

        if (inventory == null) return new NotFound();

        inventoryMainProjection = _mapper.Map<GameInventoryMainProjection>(inventory);

        var cacheOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
        var serializedInventory = JsonSerializer.Serialize(inventoryMainProjection);
        await _cache.SetStringAsync(entryKey, serializedInventory, cacheOptions, token);

        return _mapper.Map<GameInventoryMainProjection>(inventoryMainProjection);
    }
}