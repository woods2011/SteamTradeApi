using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Requests;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Infrastructure.SteamClients;

namespace SteamClientTestPolygonWebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ISteamInventoriesClient _steamInventoriesClient;
        private readonly ISteamPricesClient _steamPricesClient;
        private readonly IMemoryCache _memoryCache;

        private static readonly AsyncRetryPolicy<ApiResponse<SteamSdkInventoryResponse>> SteamInventoriesRetryPolicy =
            PolicyFactory<ApiResponse<SteamSdkInventoryResponse>>();
        
        private static readonly AsyncRetryPolicy<ApiResponse<SteamSdkItemPriceResponse>> SteamPricesRetryPolicy =
            PolicyFactory<ApiResponse<SteamSdkItemPriceResponse>>();


        public InventoryController(ISteamInventoriesClient steamInventoriesClient, ISteamPricesClient steamPricesClient,
            IMemoryCache memoryCache)
        {
            _steamInventoriesClient = steamInventoriesClient;
            _steamPricesClient = steamPricesClient;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SteamInventoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<SteamInventoryResponse>> Get([FromQuery] GetSteamInventoryRequest request)
        {
            var entryKey = $"Inventory-{request.Steam64Id}";
            if (_memoryCache.TryGetValue(entryKey, out SteamInventoryResponse? cacheValue))
                return cacheValue!;

            var executeResult = await SteamInventoriesRetryPolicy.ExecuteAndCaptureAsync(() =>
                _steamInventoriesClient.GetInventory(request.Steam64Id, 570, request.MaxCount));

            if (executeResult.Outcome is OutcomeType.Failure)
                return StatusCode(StatusCodes.Status504GatewayTimeout, "Steam API is temporary unavailable");

            var steamResponse = executeResult.Result;
            
            if (steamResponse.IsSuccessStatusCode is false) // todo: never reach this condition
                return StatusCode(StatusCodes.Status502BadGateway, $"Steam Error: {steamResponse.ReasonPhrase}" );

            var response = steamResponse.Content;

            if (response == null) return NotFound("Inventory not found");

            var itemAssetWithDescriptionResponses = response.Assets.Join(response.Descriptions,
                itemAsset => (itemAsset.ClassId, itemAsset.InstanceId),
                itemDescription => (itemDescription.ClassId, itemDescription.InstanceId),
                (itemAsset, itemDescription) =>
                    new ItemWithDescriptionResponse(itemAsset.AssetId, itemDescription.Tradable));

            var steamInventoryResponse = new SteamInventoryResponse(itemAssetWithDescriptionResponses);

            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(45));
            _memoryCache.Set(entryKey, steamInventoryResponse, cacheOptions);

            return Ok(steamInventoryResponse);
        }
        
        // Get With Prices

        private static AsyncRetryPolicy<TApiResponse> PolicyFactory<TApiResponse>() where TApiResponse : IApiResponse =>
            Policy<TApiResponse>
                .Handle<HttpRequestException>().Or<TaskCanceledException>().Or<TimeoutException>()
                .OrResult(response => response.IsSuccessStatusCode is false)
                .RetryAsync(3);
    }
}