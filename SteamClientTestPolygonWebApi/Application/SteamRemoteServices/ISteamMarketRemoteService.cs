using System.ComponentModel;
using System.Globalization;
using Refit;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.Application.SteamRemoteServices;

public interface ISteamMarketRemoteService
{
    Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsd(
        int appId,
        string marketHashName,
        CancellationToken token = default);

    Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
        int appId,
        IEnumerable<string> marketHashNames,
        CancellationToken token = default);
    
    Task<SteamServiceResult<ListingsResponse?>> GetItemMarketListings(
        int appId,
        string marketHashName,
        string? filter = null,
        int start = 0,
        int count = 10,
        SteamCurrency currency = SteamCurrency.Usd,
        CancellationToken token = default);

    Task<SteamServiceResult<GameItemMarketHistoryChartResponse?>> GetItemMarketHistory(
        int appId,
        string marketHashName,
        CancellationToken token = default);
}


public class SteamMarketRemoteService : ISteamMarketRemoteService
{
    private readonly ISteamPricesClient _steamPricesClient;

    public SteamMarketRemoteService(ISteamPricesClient steamPricesClient) =>
        _steamPricesClient = steamPricesClient;

    public async Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsd(
        int appId,
        string marketHashName,
        CancellationToken token = default)
    {
        return await SteamApiResponseToOneOfMapper.Map(() =>
            _steamPricesClient.GetItemLowestMarketPriceUsd(appId, marketHashName, token));
    }

    public async Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
        int appId,
        IEnumerable<string> marketHashNames,
        CancellationToken token = default)
    {
        const int requestsPerSec = 10;                  // ToDo: move to config
        const int maxSimultaneouslyRequestsCount = 150; // ToDo: move to config

        var steamServiceResultsDelayedTasks = await marketHashNames
            .RunTasksWithDelay(
                marketHashName => GetItemLowestMarketPriceUsd(appId, marketHashName, token),
                requestsPerSec, maxSimultaneouslyRequestsCount, token)
            .ToListAsync(cancellationToken: token);

        return await steamServiceResultsDelayedTasks.WhenAllAsync();
    }

    public async Task<SteamServiceResult<ListingsResponse?>> GetItemMarketListings(
        int appId,
        string marketHashName,
        string? filter = null,
        int start = 0,
        int count = 10,
        SteamCurrency currency = SteamCurrency.Usd,
        CancellationToken token = default)
    {
        return await SteamApiResponseToOneOfMapper.Map(() => _steamPricesClient.GetItemMarketListings(
            appId, marketHashName, filter, start, count, (int) currency, token));
    }

    public async Task<SteamServiceResult<GameItemMarketHistoryChartResponse?>> GetItemMarketHistory(
        int appId,
        string marketHashName,
        CancellationToken token = default)
    {
        var steamServiceResult = await SteamApiResponseToOneOfMapper.Map(() =>
            _steamPricesClient.GetItemMarketListingsWithHistoryRaw(appId, marketHashName, token));

        if (!steamServiceResult.TryPickT0(out var responseHtml, out var errorsReminder))
            return errorsReminder.Match<SteamServiceResult<GameItemMarketHistoryChartResponse?>>(r => r, r => r);

        if (responseHtml is null) return null as GameItemMarketHistoryChartResponse;

        var historyChartInJsArrayForm = responseHtml.Split("var line1=")[1].Split(";")[0];

        var trimExtraBracesThenSplitInputToArrayOfArrays = historyChartInJsArrayForm.Trim('[', ']').Split("],[");

        var splitEachArrayToElementsThenTrimQuotes = trimExtraBracesThenSplitInputToArrayOfArrays
            .Select(tuple => tuple.Split(",")
                .Select(tupleElement => tupleElement.Trim('\"')).ToArray());

        var historyChartPoints = splitEachArrayToElementsThenTrimQuotes
            .Select(elements => new GameItemMarketHistoryChartPointResponse(elements[0], elements[1], elements[2]));

        return new GameItemMarketHistoryChartResponse(historyChartPoints);
    }
}