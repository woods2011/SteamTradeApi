using System.Net;
using OneOf;
using OneOf.Types;
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
            
            return steamResponse switch
            {
                { IsSuccessStatusCode: true } => steamResponse.Content,
                { StatusCode: HttpStatusCode.NotFound } => new NotFound(),
                { StatusCode: HttpStatusCode.TooManyRequests } => new ProxyServersError(),
                _ => new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase)
            };
        }
        catch (Exception)
        {
            return new ProxyServersError();
        }
    }

    public static async Task<SteamServiceResult<TContent?>>
        MapWrapped<TContent>(Func<Task<ApiResponse<WrappedResponse<TContent>>>> responseFactory) where TContent : class
    {
        try
        {
            var steamResponse = await responseFactory();
            
            return steamResponse switch
            {
                { IsSuccessStatusCode: true } => steamResponse.Content?.Response,
                { StatusCode: HttpStatusCode.NotFound } => new NotFound(),
                { StatusCode: HttpStatusCode.TooManyRequests } => new ProxyServersError(),
                _ => new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase)
            };
        }
        catch (Exception)
        {
            return new ProxyServersError();
        }
    }
}

[GenerateOneOf]
public partial class SteamServiceResult<TContent> :
    OneOfBase<TContent, NotFound, ProxyServersError, SteamError> where TContent : class?
{
    public static implicit operator SteamServiceResult<TContent>(OneOf<NotFound, ProxyServersError, SteamError> errors)
        => errors.Match<SteamServiceResult<TContent>>(y => y, z => z, w => w);
}

public record struct ProxyServersError;

public record SteamError(HttpStatusCode StatusCode, string? ReasonPhrase);