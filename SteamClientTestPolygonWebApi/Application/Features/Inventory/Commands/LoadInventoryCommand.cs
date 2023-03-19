using System.ComponentModel.DataAnnotations;
using System.Net;
using MediatR;
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
    public int MaxCount { get; init; } = 5000;
}

public class LoadSteamInventoryCommandHandler : IRequestHandler<LoadInventoryCommand, LoadInventoryResult>
{
    private readonly SteamTradeApiDbContext _dbCtx;
    private readonly ISteamInventoriesClient _steamInventoriesClient;
    private readonly ITradeCooldownParserFactory _tradeCooldownParserFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LoadSteamInventoryCommandHandler> _logger;


    public LoadSteamInventoryCommandHandler(
        SteamTradeApiDbContext dbCtx,
        ISteamInventoriesClient steamInventoriesClient,
        ITradeCooldownParserFactory tradeCooldownParserFactory,
        IDateTimeProvider dateTimeProvider,
        IDistributedCache cache,
        ILogger<LoadSteamInventoryCommandHandler> logger)
    {
        _dbCtx = dbCtx;
        _steamInventoriesClient = steamInventoriesClient;
        _tradeCooldownParserFactory = tradeCooldownParserFactory;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _logger = logger;
    }


    public async Task<LoadInventoryResult> Handle(LoadInventoryCommand command, CancellationToken token)
    {
        var (steam64Id, appId) = (command.Steam64Id, command.AppId);

        var response = await _steamInventoriesClient.GetInventory(steam64Id, appId, command.MaxCount);

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
                .Select(descriptionResponse => descriptionResponse.MapToGameItem());

            await _dbCtx.BulkInsertIfNotExistsAsync(domainGameItems.ToList(), ct);
        }
    }
}

[GenerateOneOf]
public partial class LoadInventoryResult :
    OneOfBase<Upserted<GameInventory>, NotFound, ConnectionToSteamError, SteamError> { }