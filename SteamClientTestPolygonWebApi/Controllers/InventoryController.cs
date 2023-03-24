using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Queries;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Controllers.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace SteamClientTestPolygonWebApi.Controllers;

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

    /// <summary>
    /// Gets the Inventory Full Projection by Application Id and Steam Id of the User
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// GET /Inventory/76561198015469433/570
    /// </remarks>
    /// <param name="query">GetInventoryFullQuery object</param>
    /// <returns>Returns Inventory full projection</returns>
    [HttpGet("{Steam64Id}/{AppId}")]
    [SwaggerOperation(Tags = new[] { "InventoryProjections" })]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "If inventory not loaded (found) yet")]
    public async Task<ActionResult<GameInventoryFullProjection>> GetFullProjection(
        GetInventoryFullQuery query,
        CancellationToken token)
    {
        //throw new Exception("Hello From Exception");

        var inventoryResponse = await _mediatr.Send(query, token);
        return inventoryResponse.Match<ActionResult<GameInventoryFullProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound("Inventory not found, Please load it first"));
    }


    /// <summary>
    /// Gets the Inventory Split Projection by Application Id and Steam Id of the User
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// GET /Inventory/76561198015469433/570/Split
    /// </remarks>
    /// <param name="query">GetInventorySplitQuery object</param>
    /// <returns>Returns Inventory split projection</returns>
    [HttpGet("{Steam64Id}/{AppId}/Split")]
    [SwaggerOperation(Tags = new[] { "InventoryProjections" })]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "If inventory not loaded (found) yet")]
    public async Task<ActionResult<GameInventorySplitProjection>> GetSplitProjection(
        GetInventorySplitQuery query,
        CancellationToken token)
    {
        var inventoryResponse = await _mediatr.Send(query, token);
        return inventoryResponse.Match<ActionResult<GameInventorySplitProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Gets the Inventory Tradability Projection by Application Id and Steam Id of the User
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// GET /Inventory/76561198015469433/570/Tradability
    /// </remarks>
    /// <param name="query">GetInventoryTradabilityQuery object</param>
    /// <returns>Returns Inventory tradability projection</returns>
    [HttpGet("{Steam64Id}/{AppId}/Tradability")]
    [SwaggerOperation(Tags = new[] { "InventoryProjections" })]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "If inventory not found (loaded) yet")]
    public async Task<ActionResult<GameInventoryTradabilityProjection>> GetTradabilityProjection(
        GetInventoryTradabilityQuery query,
        CancellationToken token)
    {
        var inventoryResponse = await _mediatr.Send(query, token);
        return inventoryResponse.Match<ActionResult<GameInventoryTradabilityProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Creates or Update User Steam Inventory by Application Id and Steam Id of the User
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /Inventory/76561198015469433/570?MaxCount=300
    /// </remarks>
    /// <param name="command">LoadInventoryCommand object</param>
    /// <returns>Returns NoContent if updated inventory was already present in the system or Created if it's first load</returns>
    [HttpPost("{Steam64Id}/{AppId}")]
    [SwaggerResponse(201, "Successfully created")]
    [SwaggerResponse(204, "Successfully updated")]
    [SwaggerResponse(404, "If inventory not found or Hidden by user privacy settings")]
    [SwaggerResponse(502, "If Steam returns error on user request")]
    [SwaggerResponse(504, "If application proxies servers are temporary overload or unavailable")]
    public async Task<IActionResult> Load(LoadInventoryCommand command, CancellationToken token)
    {
        var loadInventoryResult = await _mediatr.Send(command, token);

        return loadInventoryResult.Match<IActionResult>(
            upsertedInventory => upsertedInventory.IsNewlyCreated
                ? CreatedAtAction(nameof(GetFullProjection), new { command.AppId, command.Steam64Id }, null)
                : NoContent(),
            notFound => NotFound(ErrorMessages.InventoryNotFound),
            proxyServersError => StatusCode(StatusCodes.Status504GatewayTimeout, ErrorMessages.ProxyServersError),
            steamError => StatusCode(StatusCodes.Status502BadGateway, ErrorMessages.SteamError(steamError)));
    }


    /// <summary>
    /// Trying to Load/Update Prices for all items in User Steam Inventory by Application Id and Steam Id of the User
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// POST /Inventory/76561198015469433/570/Prices
    /// </remarks>
    /// <param name="command">LoadInventoryItemsPricesCommand object</param>
    /// <returns>Returns NoContent</returns>
    [HttpPost("{Steam64Id}/{AppId}/Prices")]
    [SwaggerResponse(204, "Successfully updated")]
    [SwaggerResponse(404, "If inventory not loaded (found) yet")]
    public async Task<IActionResult> LoadPrices(LoadInventoryItemsPricesCommand command, CancellationToken token)
    {
        var loadInventoryPricesResult = await _mediatr.Send(command, token);

        return loadInventoryPricesResult.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(ErrorMessages.InventoryNotFound));
    }
}