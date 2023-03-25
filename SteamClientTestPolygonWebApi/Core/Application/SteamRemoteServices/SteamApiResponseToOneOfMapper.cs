using System.Net;
using OneOf;
using OneOf.Types;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;

public static class SteamApiResponseToOneOfMapper
{
    public static async Task<SteamServiceResult<TContent?>> Map<TContent>(
        Func<Task<ApiResponse<TContent>>> responseFactory) where TContent : class
    {
        ApiResponse<TContent> steamResponse;
        try
        {
            steamResponse = await responseFactory();
        }
        catch (Exception e)
        {
            return new ProxyServersError();
        }

        return steamResponse switch
        {
            { IsSuccessStatusCode: true, Error.Content: null } => null as TContent,
            { IsSuccessStatusCode: true, Error: { } error } => throw error, // e.g. deserialization error
            { IsSuccessStatusCode: true } => steamResponse.Content,
            { StatusCode: HttpStatusCode.NotFound } => new NotFound(),
            { StatusCode: HttpStatusCode.TooManyRequests } => new ProxyServersError(),
            _ => new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase)
        };
    }

    public static async Task<SteamServiceResult<TContent?>> MapWrapped<TContent>(
        Func<Task<ApiResponse<WrappedResponse<TContent>>>> responseFactory) where TContent : class
    {
        ApiResponse<WrappedResponse<TContent>> steamResponse;
        try
        {
            steamResponse = await responseFactory();
        }
        catch (Exception e)
        {
            return new ProxyServersError();
        }

        return steamResponse switch
        {
            { IsSuccessStatusCode: true, Error.Content: null } => null as TContent,
            { IsSuccessStatusCode: true, Error: { } error } => throw error, // e.g. deserialization error
            { IsSuccessStatusCode: true } => steamResponse.Content?.Response,
            { StatusCode: HttpStatusCode.NotFound } => new NotFound(),
            { StatusCode: HttpStatusCode.TooManyRequests } => new ProxyServersError(),
            _ => new SteamError(steamResponse.StatusCode, steamResponse.ReasonPhrase)
        };
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