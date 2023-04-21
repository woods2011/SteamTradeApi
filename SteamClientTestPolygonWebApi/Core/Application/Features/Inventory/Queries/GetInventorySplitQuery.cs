using System.ComponentModel.DataAnnotations;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Queries;

public class GetInventorySplitQuery : IRequest<OneOf<GameInventorySplitProjection, NotFound>>
{
    [Required]
    public long Steam64Id { get; init; }
    
    [Required]
    public int AppId { get; init; }
}

public class GetSteamInventorySplitQueryHandler :
    IRequestHandler<GetInventorySplitQuery, OneOf<GameInventorySplitProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly IMapper _mapper;

    public GetSteamInventorySplitQueryHandler(SteamTradeApiDbContext dbCtx, IMapper mapper)
    {
        _dbCtx = dbCtx;
        _mapper = mapper;
    }

    public async Task<OneOf<GameInventorySplitProjection, NotFound>> Handle(
        GetInventorySplitQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);

        var inventory = await _dbCtx.Inventories
            .Include(inv => inv.Assets).ThenInclude(asset => asset.GameItem)
            .FirstOrDefaultAsync(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId, token);

        if (inventory is null) return new NotFound();

        var assetsSplitProjection = inventory.Assets
            .Select(asset => _mapper.Map<GameInventoryAssetSplitProjection>(asset)).ToList();
        
        var gameItemsSplitProjection = inventory.Assets
            .Select(asset => asset.GameItem)
            .DistinctBy(item => item.MarketHashName)
            .Select(gameItem => _mapper.Map<GameItemFullProjection>(gameItem)).ToList();

        var inventorySplitProjection = new GameInventorySplitProjection(
            inventory.AppId,
            inventory.OwnerSteam64Id,
            inventory.LastUpdateTimeUtc,
            assetsSplitProjection,
            gameItemsSplitProjection);

        return inventorySplitProjection;
    }
}