using System.ComponentModel.DataAnnotations;
using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Core.Application.Common;
using SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Core.Domain.Item;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Commands;

public class LoadInventoryItemsPricesCommand : IRequest<OneOf<Success, NotFound>>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }
}

public class LoadInventoryItemsPricesCommandHandler :
    IRequestHandler<LoadInventoryItemsPricesCommand, OneOf<Success, NotFound>>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly ISteamMarketRemoteService _steamMarketService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LoadInventoryCommandHandler> _logger;


    public LoadInventoryItemsPricesCommandHandler(
        SteamTradeApiDbContext dbCtx,
        ISteamMarketRemoteService steamMarketService,
        IDateTimeProvider dateTimeProvider,
        IDistributedCache cache,
        ILogger<LoadInventoryCommandHandler> logger)
    {
        _dbCtx = dbCtx;
        _steamMarketService = steamMarketService;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _logger = logger;
    }


    public async Task<OneOf<Success, NotFound>> Handle(LoadInventoryItemsPricesCommand command, CancellationToken token)
    {
        var (steam64Id, appId) = (command.Steam64Id.ToString(), command.AppId);

        var utcNow = _dateTimeProvider.UtcNow;
        var thresholdUtc = utcNow.AddDays(-2); // ToDo: -2 move to config

        var doesInventoryExist = await _dbCtx.Inventories.AnyAsync(
            inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId, token);

        if (doesInventoryExist is false) return new NotFound();
        // _logger.LogWarning("Inventory not found for steam64Id: {Steam64Id} and appId: {AppId}", steam64Id, appId);

        var uniqueItemsFromInventory = await _dbCtx.Inventories
            .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId) // .Take(1)
            .SelectMany(inventory => inventory.Assets)
            .Where(asset => asset.IsMarketable)
            .Select(asset => asset.GameItem)
            .Distinct()
            .Where(item => item.PriceInfo == null || item.PriceInfo.LastUpdateUtc < thresholdUtc)
            .ToListAsync(cancellationToken: token);

        var prices = await _steamMarketService.GetItemsLowestMarketPriceUsd(
            appId, uniqueItemsFromInventory.Select(item => item.MarketHashName), token);

        foreach (var (gameItem, steamServiceResult) in uniqueItemsFromInventory.Zip(prices))
        {
            steamServiceResult.TryPickT0(out var itemPriceResponse, out var errors);
            if (itemPriceResponse?.LowestPrice is null) continue;

            var (lowestPrice, medianPrice) = (itemPriceResponse.LowestPrice, itemPriceResponse.MedianPrice);

            var newPriceInfo = PriceInfo.Create(
                ParseUsdPrice(lowestPrice),
                medianPrice is null ? null : ParseUsdPrice(medianPrice),
                utcNow);

            gameItem.UpdatePriceInfo(newPriceInfo);
        }

        await _dbCtx.SaveChangesAsync(token);
        await _cache.RemoveAsync($"InventoryMainProjection-{steam64Id}{appId}", token);
        return new Success();

        static decimal ParseUsdPrice(string price) =>
            Decimal.Parse(price, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"));
    }
}

// var uniqueItemsFromInventory = await _dbCtx.Assets
//     .Where(inv => inv.OwnerSteam64Id == steam64Id && inv.AppId == appId)
//     .Where(asset => asset.IsMarketable)
//     .Select(asset => asset.GameItem)
//     .Distinct()
//     .Where(item => item.PriceInfo == null || item.PriceInfo.LastUpdateUtc < thresholdUtc)
//     .ToListAsync(token);