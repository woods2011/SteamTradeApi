using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using SteamClientTestPolygonWebApi.Contracts;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Requests;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Helpers;
using SteamClientTestPolygonWebApi.Helpers.Extensions;

namespace SteamClientTestPolygonWebApi.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly HttpClient _steamClient;
        private readonly IMemoryCache _memoryCache;
        private readonly JsonSerializerOptions _jsonDeserializeOptions;

        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(message => message.IsSuccessStatusCode is false)
            .RetryAsync(5);


        public InventoryController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _steamClient = httpClientFactory.CreateClient("SteamClient");
            _memoryCache = memoryCache;
            _jsonDeserializeOptions = new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCaseNamingPolicy() };
        }


        [HttpGet]
        [ProducesResponseType(typeof(SteamInventoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<SteamInventoryResponse>> Get([FromQuery] GetSteamInventoryRequest request)
        {
            var entryKey = $"Inventory_{request.Steam64Id}";
            if (_memoryCache.TryGetValue(entryKey, out SteamInventoryResponse cacheValue))
                return cacheValue;

            HttpResponseMessage serializedResponse;
            try
            {
                serializedResponse = await _policy.ExecuteAsync(() =>
                    _steamClient.GetAsync($"inventory/{request.Steam64Id}/570/2?count={request.MaxCount}"));
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, "Steam API is temporary unavailable");
            }

            if (serializedResponse.IsSuccessStatusCode is false)
                return StatusCode(StatusCodes.Status502BadGateway, "Steam API is temporary unavailable");

            var response = await serializedResponse.Content
                .ReadFromJsonAsync<ExternalSteamInventoryResponse>(_jsonDeserializeOptions);

            if (response == null) return NotFound("Inventory not found");

            var itemAssetWithDescriptionResponses = response.Assets.Join(response.Descriptions,
                itemAsset => (Classid: itemAsset.ClassId, Instanceid: itemAsset.InstanceId),
                itemDescription => (Classid: itemDescription.ClassId, Instanceid: itemDescription.InstanceId),
                (itemAsset, itemDescription) =>
                    new ItemWithDescriptionResponse(itemAsset.AssetId, itemDescription.Tradable));
            var steamInventoryResponse = new SteamInventoryResponse(itemAssetWithDescriptionResponses);


            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(45));
            _memoryCache.Set(entryKey, steamInventoryResponse, cacheOptions);

            return Ok(steamInventoryResponse);
        }
    }
}