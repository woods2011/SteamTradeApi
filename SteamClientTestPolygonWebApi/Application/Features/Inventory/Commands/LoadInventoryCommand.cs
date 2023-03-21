using System.ComponentModel.DataAnnotations;
using System.Net;
using EFCore.BulkExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Mapping.ManualToDomain;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Application.SteamRemoteServices;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;

namespace SteamClientTestPolygonWebApi.Application.Features.Inventory.Commands;

public class LoadInventoryCommand : IRequest<LoadInventoryResult>
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }

    [Range(1, 5000)]
    public int MaxCount { get; init; } = 500;
}

public class LoadInventoryCommandHandler : IRequestHandler<LoadInventoryCommand, LoadInventoryResult>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly ISteamInventoriesRemoteService _steamInventoriesService;
    private readonly ITradeCooldownParserFactory _tradeCooldownParserFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LoadInventoryCommandHandler> _logger;


    public LoadInventoryCommandHandler(
        SteamTradeApiDbContext dbCtx,
        ISteamInventoriesRemoteService steamInventoriesService,
        ITradeCooldownParserFactory tradeCooldownParserFactory,
        IDateTimeProvider dateTimeProvider,
        IDistributedCache cache,
        ILogger<LoadInventoryCommandHandler> logger)
    {
        _dbCtx = dbCtx;
        _steamInventoriesService = steamInventoriesService;
        _tradeCooldownParserFactory = tradeCooldownParserFactory;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _logger = logger;
    }


    public async Task<LoadInventoryResult> Handle(LoadInventoryCommand command, CancellationToken token)
    {
        var (steam64Id, appId) = (command.Steam64Id, command.AppId);

        var response = await _steamInventoriesService.GetInventory(steam64Id, appId, command.MaxCount, token);

        return await response.Match<Task<LoadInventoryResult>>(
            async content => content is null ? new NotFound() : await UpsertInventory(content),
            connectionToSteamError => Task.FromResult<LoadInventoryResult>(connectionToSteamError),
            steamError => steamError.StatusCode
                is HttpStatusCode.NotFound or HttpStatusCode.Forbidden
                ? Task.FromResult<LoadInventoryResult>(new NotFound())
                : Task.FromResult<LoadInventoryResult>(steamError));


        async Task<Upserted<GameInventory>> UpsertInventory(SteamSdkInventoryResponse inventoryResponse)
        {
            await AddNewItemTypes(inventoryResponse.Descriptions, token);

            var tradeCooldownParser = _tradeCooldownParserFactory.Create(appId);
            var inventoryAssetsDomain = inventoryResponse.MapToGameInventoryAssets(steam64Id, tradeCooldownParser);

            var inventoryCompositePk = new object[] { steam64Id.ToString(), appId };
            var existingInventoryDomain = await _dbCtx.Inventories.FindAsync(inventoryCompositePk, token);

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

        async Task AddNewItemTypes(IEnumerable<SteamSdkDescriptionResponse> descriptions, CancellationToken ct)
        {
            var domainGameItems = descriptions
                .DistinctBy(descriptionResponse => descriptionResponse.MarketHashName)
                .Select(descriptionResponse => descriptionResponse.MapToGameItem())
                .ToList();

            var allNames = domainGameItems.Select(item => item.MarketHashName).ToList();
            var existedNames = await _dbCtx.Items.Where(item => allNames.Contains(item.MarketHashName))
                .Select(item => item.MarketHashName).ToListAsync(ct);
            var itemsToInsert = domainGameItems.Where(item => !existedNames.Contains(item.MarketHashName));

            await _dbCtx.BulkInsertAsync(itemsToInsert.ToList(), cancellationToken: ct);
            //await _dbCtx.BulkInsertIfNotExistsAsync(itemsToInsert.ToList(), ct); // TODO: find fix for this
        }
    }
}

[GenerateOneOf]
public partial class LoadInventoryResult :
    OneOfBase<Upserted<GameInventory>, NotFound, ConnectionToSteamError, SteamError> { }