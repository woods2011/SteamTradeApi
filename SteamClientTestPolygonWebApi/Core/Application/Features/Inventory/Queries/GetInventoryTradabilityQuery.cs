using System.ComponentModel.DataAnnotations;
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

public class GetInventoryTradabilityQuery : IRequest<OneOf<GameInventoryTradabilityProjection, NotFound>>
{
    [Required]
    public long Steam64Id { get; init; }

    [Required]
    public int AppId { get; init; }
}

public class GetInventoryTradabilityQueryHandler :
    IRequestHandler<GetInventoryTradabilityQuery, OneOf<GameInventoryTradabilityProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;

    public GetInventoryTradabilityQueryHandler(SteamTradeApiDbContext dbCtx) => _dbCtx = dbCtx;

    public async Task<OneOf<GameInventoryTradabilityProjection, NotFound>> Handle(
        GetInventoryTradabilityQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);

        GameInventoryTradabilityProjection? inventoryProjection = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .ProjectToType<GameInventoryTradabilityProjection>()
            .FirstOrDefaultAsync(token);

        if (inventoryProjection is null) return new NotFound();

        return inventoryProjection;
    }
}