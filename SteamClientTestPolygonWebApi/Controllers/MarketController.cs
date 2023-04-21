using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteamClientTestPolygonWebApi.Core.Application.Features.Market.Queries;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Controllers.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace SteamClientTestPolygonWebApi.Controllers;

[Produces("application/json")]
[ApiController]
[Route("[controller]")]
public class MarketController : ControllerBase
{
    private readonly ISender _mediatr;
    private readonly ILogger<MarketController> _logger;

    public MarketController(ISender mediatr, ILogger<MarketController> logger)
    {
        _mediatr = mediatr;
        _logger = logger;
    }

    /// <summary>
    /// Gets the history of specified Item price changes by Application Id and MarketHashName of the Item
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /Market/PriceHistory/570/The Abscesserator
    /// 
    /// </remarks>
    /// <param name="query">GetItemMarketHistoryQuery object</param>
    /// <returns>Returns Item price chart</returns>
    [HttpGet("PriceHistory/{AppId}/{MarketHashName}")]
    [SwaggerResponse(200, "Success created")]
    [SwaggerResponse(404, "If inventory not found or Hidden by user privacy settings")]
    [SwaggerResponse(502, "If Steam returns error on user request")]
    [SwaggerResponse(504, "If application proxies servers are temporary overload or unavailable")]
    public async Task<ActionResult<GameItemMarketHistoryChartResponse>> GetItemMarketHistory(
        GetItemMarketHistoryQuery query,
        CancellationToken token)
    {
        var itemMarketHistoryQueryResult = await _mediatr.Send(query, token);
        return itemMarketHistoryQueryResult.Match<ActionResult<GameItemMarketHistoryChartResponse>>(
            itemMarketHistoryResponse => itemMarketHistoryResponse,
            notFound => NotFound(ErrorMessages.ItemNotFoundOnMarket),
            proxyServersError => StatusCode(StatusCodes.Status504GatewayTimeout, ErrorMessages.ProxyServersError),
            steamError => StatusCode(StatusCodes.Status502BadGateway, ErrorMessages.SteamError(steamError)));
    }


    /// <summary>
    /// Gets Top10 lowest price listings of specified Item by Application Id and MarketHashName of the Item and optional Filter string
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /Market/Listings/570/Skull of the Razorwyrm?Filter=Skull of the Razorwyrm NOT "Chaotic locked"
    /// 
    /// </remarks>
    /// <param name="query">GetItemMarketListingsQuery object</param>
    /// <returns>Returns Top10 lowest price listings of specified Item</returns>
    [HttpGet("Listings/{AppId}/{MarketHashName}")]
    [SwaggerResponse(200, "Success created")]
    [SwaggerResponse(404, "If inventory not found or Hidden by user privacy settings")]
    [SwaggerResponse(502, "If Steam returns error on user request")]
    [SwaggerResponse(504, "If application proxies servers are temporary overload or unavailable")]
    public async Task<ActionResult<GameItemMarketListingsResponse>> GetItemMarketListings(
        GetItemMarketListingsQuery query,
        CancellationToken token)
    {
        var itemMarketListingsQueryResult = await _mediatr.Send(query, token);
        return itemMarketListingsQueryResult.Match<ActionResult<GameItemMarketListingsResponse>>(
            itemMarketListingsResponse => itemMarketListingsResponse,
            notFound => NotFound(ErrorMessages.ItemNotFoundOnMarket),
            proxyServersError => StatusCode(StatusCodes.Status504GatewayTimeout, ErrorMessages.ProxyServersError),
            steamError => StatusCode(StatusCodes.Status502BadGateway, ErrorMessages.SteamError(steamError)));
    }
}