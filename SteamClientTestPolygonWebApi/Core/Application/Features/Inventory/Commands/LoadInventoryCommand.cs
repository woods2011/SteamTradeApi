using System.ComponentModel.DataAnnotations;
using System.Net;
using EFCore.BulkExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Core.Application.Common;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Mapping.ManualToDomain;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Core.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Core.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Core.Domain.Item;
using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Commands;

public class LoadInventoryCommand : IRequest<LoadInventoryResult>
{
    [Required]
    public long Steam64Id { get; init; }

    [Required]
    public int AppId { get; init; }

    [Range(1, 7000)]
    public int MaxCount { get; init; } = 500;
}

public class LoadInventoryCommandHandler : IRequestHandler<LoadInventoryCommand, LoadInventoryResult>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly ISteamInventoriesRemoteService _steamInventoriesService;
    private readonly ITradeCooldownParserFactory _tradeCooldownParserFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDistributedCache _cache;


    public LoadInventoryCommandHandler(
        SteamTradeApiDbContext dbCtx,
        ISteamInventoriesRemoteService steamInventoriesService,
        ITradeCooldownParserFactory tradeCooldownParserFactory,
        IDateTimeProvider dateTimeProvider,
        IDistributedCache cache)
    {
        _dbCtx = dbCtx;
        _steamInventoriesService = steamInventoriesService;
        _tradeCooldownParserFactory = tradeCooldownParserFactory;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
    }


    public async Task<LoadInventoryResult> Handle(LoadInventoryCommand command, CancellationToken token)
    {
        var (steam64Id, appId) = (command.Steam64Id, command.AppId);

        SteamServiceResult<SteamSdkInventoryResponse?> steamServiceResult =
            await _steamInventoriesService.GetInventory(steam64Id, appId, command.MaxCount, token);

        if (!steamServiceResult.TryPickResult(out SteamSdkInventoryResponse? steamSdkInventoryResponse, out var errors))
            return errors.Match<LoadInventoryResult>(
                notFound => notFound,
                error => error,
                steamError => steamError.StatusCode is HttpStatusCode.Forbidden ? new NotFound() : steamError);


        if (steamSdkInventoryResponse is null) return new NotFound();

        await AddNewItemTypes(steamSdkInventoryResponse.Descriptions);
        return await UpsertInventory(steamSdkInventoryResponse);


        async Task<Upserted<GameInventory>> UpsertInventory(SteamSdkInventoryResponse inventoryResponse)
        {
            ITradeCooldownParser tradeCooldownParser = _tradeCooldownParserFactory.Create(appId);
            List<GameInventoryAsset> inventoryAssetsDomain =
                inventoryResponse.MapToGameInventoryAssets(steam64Id, tradeCooldownParser);

            var inventoryCompositePk = new object[] { steam64Id.ToString(), appId };
            GameInventory? existingInventoryDomain = await _dbCtx.Inventories.FindAsync(inventoryCompositePk, token);

            if (existingInventoryDomain is not null)
            {
                existingInventoryDomain.UpdateInventory(inventoryAssetsDomain, _dateTimeProvider.UtcNow);
                _dbCtx.Inventories.Update(existingInventoryDomain);
                await _dbCtx.SaveChangesAsync(token);
                await _cache.RemoveAsync($"InventoryMainProjection-{steam64Id}{appId}", token);
                return new Upserted<GameInventory>(existingInventoryDomain, false);
            }

            var newInventoryDomain = GameInventory.Create(
                appId: appId,
                ownerSteam64Id: steam64Id.ToString(),
                lastUpdateDateTimeUtc: _dateTimeProvider.UtcNow,
                assets: inventoryAssetsDomain);

            _dbCtx.Inventories.Add(newInventoryDomain);
            await _dbCtx.SaveChangesAsync(token);
            return new Upserted<GameInventory>(newInventoryDomain, true);
        }

        async Task AddNewItemTypes(IEnumerable<SteamSdkDescriptionResponse> descriptions)
        {
            List<GameItem> domainGameItems = descriptions
                .DistinctBy(descriptionResponse => descriptionResponse.MarketHashName)
                .Select(descriptionResponse => descriptionResponse.MapToGameItem())
                .ToList();

            List<string> allNames = domainGameItems.Select(item => item.MarketHashName).ToList();
            List<string> existedNames = await _dbCtx.Items.Where(item => allNames.Contains(item.MarketHashName))
                .Select(item => item.MarketHashName).ToListAsync(token);
            IEnumerable<GameItem> itemsToInsert =
                domainGameItems.Where(item => !existedNames.Contains(item.MarketHashName));

            await _dbCtx.BulkInsertAsync(itemsToInsert.ToList(), cancellationToken: token);
            //await _dbCtx.BulkInsertIfNotExistsAsync(itemsToInsert.ToList(), ct); // TODO: find fix for this
        }
    }
}

[GenerateOneOf]
public partial class LoadInventoryResult :
    OneOfBase<Upserted<GameInventory>, NotFound, ProxyServersError, SteamError>
{
    public static implicit operator LoadInventoryResult(OneOf<NotFound, ProxyServersError, SteamError> errors)
        => errors.Match<LoadInventoryResult>(y => y, z => z, w => w);
}