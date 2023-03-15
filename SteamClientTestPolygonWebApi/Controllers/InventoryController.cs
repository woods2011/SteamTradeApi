using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Refit;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Application.Mappers.ManualToDomain;
using SteamClientTestPolygonWebApi.Application.Utils;
using SteamClientTestPolygonWebApi.Application.Utils.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Requests;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Domain.GameItemAggregate;
using SteamClientTestPolygonWebApi.Infrastructure.Persistence;
using SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

namespace SteamClientTestPolygonWebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly SteamTradeApiDbContext _dbCtx;
        private readonly ISteamInventoriesClient _steamInventoriesClient;
        private readonly ITradeCooldownParserFactory _tradeCooldownParserFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDistributedCache _cache;
        private readonly ILogger<InventoryController> _logger;

        private static readonly AsyncRetryPolicy<ApiResponse<SteamSdkInventoryResponse>> SteamInventoriesRetryPolicy =
            PolicyFactory<ApiResponse<SteamSdkInventoryResponse>>();

        private static readonly AsyncRetryPolicy<ApiResponse<SteamSdkItemPriceResponse>> SteamPricesRetryPolicy =
            PolicyFactory<ApiResponse<SteamSdkItemPriceResponse>>();

        public InventoryController(
            SteamTradeApiDbContext dbCtx,
            ISteamInventoriesClient steamInventoriesClient,
            ITradeCooldownParserFactory tradeCooldownParserFactory,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            ILogger<InventoryController> logger)
        {
            _dbCtx = dbCtx;
            _steamInventoriesClient = steamInventoriesClient;
            _tradeCooldownParserFactory = tradeCooldownParserFactory;
            _dateTimeProvider = dateTimeProvider;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GeneralGameInventoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GeneralGameInventoryResponse>> Get([FromQuery] GetSteamInventoryQuery query,
            CancellationToken token)
        {
            var entryKey = $"InventoryGeneralQuery-{query.AppId}{query.Steam64Id}";
            var cachedResponse = await _cache.GetStringAsync(entryKey, token);
            if (cachedResponse != null)
            {
                var deserializedResponse = JsonSerializer.Deserialize<GeneralGameInventoryResponse>(cachedResponse);
                return Ok(deserializedResponse);
            }

            // ToDo: Вынести логику в query Handler или App service
            var inventoryResponse = await _dbCtx.Inventories.AsNoTracking() // As no tracking можно удалить
                .Where(inv => inv.OwnerSteam64Id == query.Steam64Id.ToString() && inv.AppId == query.AppId)
                .Select(inventory => new GeneralGameInventoryResponse(
                    inventory.AppId,
                    inventory.OwnerSteam64Id,
                    inventory.LastUpdateTimeUtc,
                    inventory.Assets
                        //      .OrderBy(asset => asset.ItemMarketHashName)
                        //      .Skip(start).Take(count) // можно добавить пагинацию, но вообще говоря она не нужна
                        .Select(asset => new GeneralGameInventoryAssetResponse(
                            asset.AssetId,
                            asset.AppId,
                            asset.ItemMarketHashName,
                            asset.OwnerSteam64Id,
                            asset.IsTradable,
                            asset.TradeCooldownUntilUtc,
                            asset.IsMarketable,
                            asset.InstanceId)).ToList()))
                .FirstOrDefaultAsync(token);

            // ToDo: Add Match result с помощью OneOf либо FluentResult либо ErrorOr
            if (inventoryResponse == null) return NotFound("Inventory not found or not loaded");

            var cacheOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20));
            var serializedInventoryResponse = JsonSerializer.Serialize(inventoryResponse);
            await _cache.SetStringAsync(entryKey, serializedInventoryResponse, cacheOptions, token);

            return Ok(inventoryResponse);
        }


        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult> Load([FromQuery] LoadSteamInventoryCommand command,
            CancellationToken token)
        {
            // ToDo: Вынести логику в Command Handler или App service
            var executeResult = await SteamInventoriesRetryPolicy.ExecuteAndCaptureAsync(() =>
                _steamInventoriesClient.GetInventory(command.Steam64Id, command.AppId, command.MaxCount));
            if (executeResult.Outcome is OutcomeType.Failure)
                return StatusCode(StatusCodes.Status504GatewayTimeout, "Steam API is temporary unavailable");

            var steamResponse = executeResult.Result;
            if (steamResponse.IsSuccessStatusCode is false)
                return StatusCode(StatusCodes.Status502BadGateway, $"Steam Error: {steamResponse.ReasonPhrase}");

            var inventoryResponse = steamResponse.Content;
            if (inventoryResponse == null) return NotFound("Inventory not found or Hidden by privacy settings");

            await AddNewItemTypes(inventoryResponse.Descriptions, token);

            var tradeCooldownParser = _tradeCooldownParserFactory.Create(command.AppId);
            var inventoryAssetsDomain = inventoryResponse
                .MapToGameInventoryAssets(command, _dateTimeProvider.UtcNow, tradeCooldownParser);
            var inventoryDomain =
                await _dbCtx.Inventories.FindAsync(new object[] { command.Steam64Id.ToString(), command.AppId }, token);

            if (inventoryDomain is not null)
            {
                inventoryDomain.UpdateInventory(inventoryAssetsDomain, _dateTimeProvider.UtcNow);
                _dbCtx.Inventories.Update(inventoryDomain);
                await _dbCtx.SaveChangesAsync(token);
                await _cache.RemoveAsync($"InventoryGeneralQuery-{command.AppId}{command.Steam64Id}", token);
                // Add Match result либо OneOf либо FluentResult либо ErrorOr
                return NoContent();
            }

            var newInventoryDomain = GameInventory.Create(
                appId: command.AppId,
                ownerSteam64Id: command.Steam64Id.ToString(),
                lastUpdateDateTimeUtc: _dateTimeProvider.UtcNow,
                assets: inventoryAssetsDomain);

            _dbCtx.Inventories.Add(newInventoryDomain);
            await _dbCtx.SaveChangesAsync(token);
            return CreatedAtAction(
                actionName: nameof(Get),
                routeValues: new { appId = command.AppId, Steam64Id = command.Steam64Id },
                value: inventoryDomain);

            // ----------------- Local functions -----------------
            async Task AddNewItemTypes(IEnumerable<SteamSdkDescriptionResponse> descriptions, CancellationToken ct)
            {
                var domainGameItems = descriptions
                    .DistinctBy(descriptionResponse => descriptionResponse.MarketHashName)
                    .Select(descriptionResponse => descriptionResponse.MapToGameItem());

                await _dbCtx.BulkInsertIfNotExistsAsync(domainGameItems.ToList(), ct);
            }
        }


        private static AsyncRetryPolicy<TApiResponse> PolicyFactory<TApiResponse>() where TApiResponse : IApiResponse =>
            Policy<TApiResponse>
                .Handle<HttpRequestException>().Or<OperationCanceledException>().Or<TimeoutException>()
                .OrResult(response => response.StatusCode >= HttpStatusCode.InternalServerError)
                .RetryAsync(5);
    }
}