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

public class GetInventoryStackPriceQuery : IRequest<OneOf<GameInventoryStackPriceProjection, NotFound>>
{
    [Required]
    public long Steam64Id { get; init; }

    [Required]
    public int AppId { get; init; }
}

public class GetInventoryStackPriceQueryHandler :
    IRequestHandler<GetInventoryStackPriceQuery, OneOf<GameInventoryStackPriceProjection, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;

    public GetInventoryStackPriceQueryHandler(SteamTradeApiDbContext dbCtx) => _dbCtx = dbCtx;

    public async Task<OneOf<GameInventoryStackPriceProjection, NotFound>> Handle(
        GetInventoryStackPriceQuery query,
        CancellationToken token)
    {
        var (steam64Id, appId) = (query.Steam64Id.ToString(), query.AppId);

        var isInventoriesExist = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .AnyAsync(token);

        if (isInventoriesExist is false) return new NotFound();
    
        List<GameItemStackPriceProjection> gameItemStackPriceProjections = await _dbCtx.Inventories
            .Include(inv => inv.Assets).ThenInclude(asset => asset.GameItem)
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
            .SelectMany(inventory => inventory.Assets)
            .Where(asset => asset.IsMarketable)
            .Where(asset => asset.GameItem.PriceInfo != null) // ToDo: add specification pattern
            .GroupBy(asset => asset.ItemMarketHashName)
            .Select(grouping => new GameItemStackPriceProjection(
                grouping.First().ItemMarketHashName,
                grouping.Count(),
                grouping.First().GameItem.PriceInfo!.LowestMarketPriceUsd,
                grouping.First().GameItem.ClassId))
            .ToListAsync(token);

        var inventoryStackPriceProjection = new GameInventoryStackPriceProjection(
            appId,
            steam64Id,
            gameItemStackPriceProjections.OrderByDescending(projection => projection.StackPriceUsd).ToList());

        return inventoryStackPriceProjection;
    }
}