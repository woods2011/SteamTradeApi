using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Queries;
using SteamClientTestPolygonWebApi.Contracts.Responses;

namespace SteamClientTestPolygonWebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ISender _mediatr;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ISender mediatr, ILogger<InventoryController> logger)
        {
            _mediatr = mediatr;
            _logger = logger;
        }


        [HttpGet("{Steam64Id}/{AppId}")]
        [ProducesResponseType(typeof(GameInventoryFullProjection), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameInventoryFullProjection>> GetFullProjection(
            GetInventoryFullQuery query,
            CancellationToken token)
        {
            var inventoryResponse = await _mediatr.Send(query, token);
            return inventoryResponse.Match<ActionResult<GameInventoryFullProjection>>(
                inventoryProjection => inventoryProjection,
                notFound => NotFound("Inventory not found, Please load it first"));
        }


        [HttpGet("{Steam64Id}/{AppId}/Split")]
        [ProducesResponseType(typeof(GameInventorySplitProjection), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameInventorySplitProjection>> GetSplitProjection(
            GetInventorySplitQuery query,
            CancellationToken token)
        {
            var inventoryResponse = await _mediatr.Send(query, token);
            return inventoryResponse.Match<ActionResult<GameInventorySplitProjection>>(
                inventoryProjection => inventoryProjection,
                notFound => NotFound("Inventory not found, Please load it first"));
        }


        [HttpGet("{Steam64Id}/{AppId}/Tradability")]
        [ProducesResponseType(typeof(GameInventoryTradabilityProjection), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameInventoryTradabilityProjection>> GetTradabilityProjection(
            GetInventoryTradabilityQuery query,
            CancellationToken token)
        {
            var inventoryResponse = await _mediatr.Send(query, token);
            return inventoryResponse.Match<ActionResult<GameInventoryTradabilityProjection>>(
                inventoryProjection => inventoryProjection,
                notFound => NotFound("Inventory not found, Please load it first"));
        }


        [HttpPost("{Steam64Id}/{AppId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult> Load(LoadInventoryCommand command, CancellationToken token)
        {
            var loadInventoryResult = await _mediatr.Send(command, token);

            return loadInventoryResult.Match<ActionResult>(
                upsertedInventory => upsertedInventory.IsNewlyCreated
                    ? CreatedAtAction(nameof(GetFullProjection), new { command.AppId, command.Steam64Id }, null) // mb return inv
                    : NoContent(),
                notFound => NotFound("Inventory not found or Hidden by privacy settings"),
                connectionToSteamError => StatusCode(StatusCodes.Status504GatewayTimeout,
                    "Our Proxies Servers are temporary unavailable or Steam is down"),
                steamError => StatusCode(StatusCodes.Status502BadGateway,
                    $"Steam Response Status Code: {steamError.StatusCode}; Error Reason: {steamError.ReasonPhrase}"));
        }


        [HttpPost("{Steam64Id}/{AppId}/Prices")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> LoadPrices(LoadInventoryItemsPricesCommand command, CancellationToken token)
        {
            var loadInventoryPricesResult = await _mediatr.Send(command, token);

            return loadInventoryPricesResult.Match<ActionResult>(
                success => NoContent(),
                notFound => NotFound("Inventory not found or Hidden by privacy settings"));
        }
    }
}