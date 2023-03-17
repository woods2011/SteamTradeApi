using System.Net;
using OneOf;
using Refit;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public static class SteamApiResponseToOneOfMapper
{
    public static async Task<SteamClientResult<TContent?>>
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
}

[GenerateOneOf]
public partial class SteamClientResult<TContent> 
    : OneOfBase<TContent, ConnectionToSteamError, SteamError> where TContent : class? { }


public record struct ConnectionToSteamError;

public record SteamError(HttpStatusCode StatusCode, string? ReasonPhrase);