using System.Net;
using OneOf;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public static class SteamApiResponseToOneOfMapper
{
    public static async Task<SteamServiceResult<TContent?>>
        Map<TContent>(Func<Task<ApiResponse<TContent>>> responseFactory) where TContent : class
    {
        try
        {
            var steamResponse = await responseFactory();
            if (!steamResponse.IsSuccessStatusCode)
                return new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase);

            return steamResponse.Content;
        }
        catch (Exception)
        {
            return new ConnectionToSteamError();
        }
    }
    
    public static async Task<SteamServiceResult<TContent?>>
        MapWrapped<TContent>(Func<Task<ApiResponse<WrappedResponse<TContent>>>> responseFactory) where TContent : class
    {
        try
        {
            var steamResponse = await responseFactory();
            if (!steamResponse.IsSuccessStatusCode)
                return new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase);

            return steamResponse.Content?.Response;
        }
        catch (Exception)
        {
            return new ConnectionToSteamError();
        }
    }
}

[GenerateOneOf]
public partial class SteamServiceResult<TContent> : 
    OneOfBase<TContent, ConnectionToSteamError, SteamError> where TContent : class? { }


public record struct ConnectionToSteamError;

public record SteamError(HttpStatusCode StatusCode, string? ReasonPhrase);