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

namespace SteamClientTestPolygonWebApi.Application.Features.Inventory.Queries;

public class GetInventoryTradabilityQuery : IRequest<OneOf<GameInventoryTradabilityProjection, NotFound>>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }
}

public class GetInventoryTradabilityQueryHandler :
    IRequestHandler<GetInventoryTradabilityQuery, OneOf<GameInventoryTradabilityProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetSteamInventoryFullQueryHandler> _logger;

    public GetInventoryTradabilityQueryHandler(
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

    public async Task<OneOf<GameInventoryTradabilityProjection, NotFound>> Handle(
        GetInventoryTradabilityQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);

        var inventoryProjection = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .ProjectToType<GameInventoryTradabilityProjection>()
            .FirstOrDefaultAsync(token);

        if (inventoryProjection is null) return new NotFound();

        return inventoryProjection;
    }
}