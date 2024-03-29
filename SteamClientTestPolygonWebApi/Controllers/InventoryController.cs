﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Controllers.Common;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace SteamClientTestPolygonWebApi.Controllers;

[Produces("application/json")]
[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ISender _mediatr;
    public InventoryController(ISender mediatr) => _mediatr = mediatr;

    /// <summary>
    /// Gets the Inventory Full Projection by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /Inventory/76561198015469433/570
    /// 
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
        OneOf<GameInventoryFullProjection, NotFound> inventoryResult = await _mediatr.Send(query, token);
        return inventoryResult.Match<ActionResult<GameInventoryFullProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Gets the Inventory Items stack price Projection by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /Inventory/76561198015469433/570/StackPrice
    /// 
    /// </remarks>
    /// <param name="query">GetInventoryStackPriceQuery object</param>
    /// <returns>Returns Inventory items stack price projection</returns>
    [HttpGet("{Steam64Id}/{AppId}/StackPrice")]
    [SwaggerOperation(Tags = new[] { "InventoryProjections" })]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "If inventory not loaded (found) yet")]
    public async Task<ActionResult<GameInventoryStackPriceProjection>> GetStackPriceProjection(
        GetInventoryStackPriceQuery query,
        CancellationToken token)
    {
        OneOf<GameInventoryStackPriceProjection, NotFound> inventoryResult = await _mediatr.Send(query, token);
        return inventoryResult.Match<ActionResult<GameInventoryStackPriceProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Gets the Inventory Split Projection by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /Inventory/76561198015469433/570/Split
    /// 
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
        OneOf<GameInventorySplitProjection, NotFound> inventoryResult = await _mediatr.Send(query, token);
        return inventoryResult.Match<ActionResult<GameInventorySplitProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Gets the Inventory Tradability Projection by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /Inventory/76561198015469433/570/Tradability
    /// 
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
        OneOf<GameInventoryTradabilityProjection, NotFound> inventoryResult = await _mediatr.Send(query, token);
        return inventoryResult.Match<ActionResult<GameInventoryTradabilityProjection>>(
            inventoryProjection => inventoryProjection,
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }


    /// <summary>
    /// Creates or Updates Steam User Inventory by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /Inventory/76561198015469433/570?MaxCount=300
    /// 
    /// </remarks>
    /// <param name="command">LoadInventoryCommand object</param>
    /// <returns>Returns NoContent if updated inventory was already present in the system or Created if it's first load</returns>
    [HttpPost("{Steam64Id}/{AppId}")]
    [SwaggerResponse(201, "Successfully created")]
    [SwaggerResponse(204, "Successfully updated")]
    [SwaggerResponse(404, "If inventory not found or Hidden by user privacy settings")]
    [SwaggerResponse(502, "If Steam returns error on user request")]
    [SwaggerResponse(504, "If application proxies servers are temporary overload or unavailable")]
    public async Task<IActionResult> LoadInventory(LoadInventoryCommand command, CancellationToken token)
    {
        LoadInventoryResult loadInventoryResult = await _mediatr.Send(command, token);

        return loadInventoryResult.Match<IActionResult>(
            upsertedInventory => upsertedInventory.IsNewlyCreated
                ? CreatedAtAction(nameof(GetFullProjection), new { command.AppId, command.Steam64Id }, null)
                : NoContent(),
            notFound => NotFound(ErrorMessages.InventoryNotFound),
            proxyServersError => StatusCode(StatusCodes.Status504GatewayTimeout, ErrorMessages.ProxyServersError),
            steamError => StatusCode(StatusCodes.Status502BadGateway, ErrorMessages.SteamError(steamError)));
    }


    /// <summary>
    /// Trying to Load/Update Prices for all items in User Steam Inventory by Steam64 User Id and Application Id
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /Inventory/76561198015469433/570/Prices
    /// 
    /// </remarks>
    /// <param name="command">LoadInventoryItemsPricesCommand object</param>
    /// <returns>Returns NoContent</returns>
    [HttpPost("{Steam64Id}/{AppId}/Prices")]
    [SwaggerResponse(204, "Successfully updated")]
    [SwaggerResponse(404, "If inventory not loaded (found) yet")]
    public async Task<IActionResult> LoadInventoryPrices(
        LoadInventoryItemsPricesCommand command,
        CancellationToken token)
    {
        OneOf<Success, NotFound> loadInventoryPricesResult = await _mediatr.Send(command, token);

        return loadInventoryPricesResult.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(ErrorMessages.InventoryNotLoaded));
    }
}