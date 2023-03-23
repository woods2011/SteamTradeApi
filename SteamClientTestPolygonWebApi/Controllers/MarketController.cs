using MediatR;
using Microsoft.AspNetCore.Mvc;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Application.Features.Inventory.Queries;
using SteamClientTestPolygonWebApi.Application.Features.Market.Queries;
using SteamClientTestPolygonWebApi.Contracts.Responses;

namespace SteamClientTestPolygonWebApi.Controllers
{
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


        [HttpGet("pricehistory/{AppId}/{MarketHashName}")]
        [ProducesResponseType(typeof(GameItemMarketHistoryChartResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<GameItemMarketHistoryChartResponse>> GetItemMarketHistory(
            GetItemMarketHistoryQuery query,
            CancellationToken token)
        {
            var itemMarketHistoryQueryResult = await _mediatr.Send(query, token);
            return itemMarketHistoryQueryResult.Match<ActionResult<GameItemMarketHistoryChartResponse>>(
                itemMarketHistoryResponse => itemMarketHistoryResponse,
                notFound => NotFound("Item not found on market, please check Item name typed correctly"),
                connectionToSteamError => StatusCode(StatusCodes.Status504GatewayTimeout,
                    "Our Proxies Servers are temporary unavailable or Steam is down"),
                steamError => StatusCode(StatusCodes.Status502BadGateway,
                    $"Steam Response Status Code: {(int) steamError.StatusCode}; Error Reason: {steamError.ReasonPhrase}"));
        }


        [HttpGet("listings/{AppId}/{MarketHashName}")]
        [ProducesResponseType(typeof(GameItemMarketListingsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<GameItemMarketListingsResponse>> GetItemMarketListings(
            GetItemMarketListingsQuery query,
            CancellationToken token)
        {
            var itemMarketListingsQueryResult = await _mediatr.Send(query, token);
            return itemMarketListingsQueryResult.Match<ActionResult<GameItemMarketListingsResponse>>(
                itemMarketListingsResponse => itemMarketListingsResponse,
                notFound => NotFound("Item not found on market, please check Item name typed correctly"),
                connectionToSteamError => StatusCode(StatusCodes.Status504GatewayTimeout,
                    "Our Proxies Servers are temporary unavailable or Steam is down"),
                steamError => StatusCode(StatusCodes.Status502BadGateway,
                    $"Steam Response Status Code: {(int) steamError.StatusCode}; Error Reason: {steamError.ReasonPhrase}"));
        }
    }
}