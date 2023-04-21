using OneOf.Types;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Responses;
using SteamClientTestPolygonWebApi.Helpers.Extensions;
using SteamClientTestPolygonWebApi.Infrastructure.SteamRefitClients;

namespace SteamClientTestPolygonWebApi.Core.Application.SteamRemoteServices;

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

    public async Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsdOnce(
        int appId,
        string marketHashName,
        CancellationToken token = default)
    {
        return await SteamApiResponseToOneOfMapper.Map(() =>
            _steamPricesClient.GetItemLowestMarketPriceUsd(appId, marketHashName, token));
    }

    public async Task<SteamServiceResult<SteamSdkItemPriceResponse?>> GetItemLowestMarketPriceUsd(
        int appId,
        string marketHashName,
        CancellationToken token = default)
    {
        SteamServiceResult<SteamSdkItemPriceResponse?> firstCompletedResult =
            await MyTaskExtensions.WhenFirstSuccessCancelOther(
                ct => GetItemLowestMarketPriceUsdOnce(appId, marketHashName, ct),
                repeatTimes: 3,
                token);

        return firstCompletedResult;
    }

    public async Task<IReadOnlyList<SteamServiceResult<SteamSdkItemPriceResponse?>>> GetItemsLowestMarketPriceUsd(
        int appId,
        IEnumerable<string> marketHashNames,
        CancellationToken token = default)
    {
        const int requestsPerSec = 10;                  // ToDo: move to config
        const int maxSimultaneouslyRequestsCount = 150; // ToDo: move to config

        List<Task<SteamServiceResult<SteamSdkItemPriceResponse?>>> steamServiceResultsDelayedTasks =
            await marketHashNames
                .RunTasksWithDelay(
                    marketHashName => GetItemLowestMarketPriceUsdOnce(appId, marketHashName, token),
                    requestsPerSec, maxSimultaneouslyRequestsCount, token)
                .ToListAsync(cancellationToken: token);

        return await steamServiceResultsDelayedTasks.WhenAll();
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
        SteamServiceResult<ListingsResponse?> firstCompletedResult = await MyTaskExtensions.WhenFirstSuccessCancelOther(
            ct => GetItemMarketListingsOnce(appId, marketHashName, filter, start, count, currency, ct),
            repeatTimes: 3,
            token);

        return firstCompletedResult;
    }

    public async Task<SteamServiceResult<ListingsResponse?>> GetItemMarketListingsOnce(
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
        SteamServiceResult<string?> steamServiceResult = await SteamApiResponseToOneOfMapper.Map(
            () => _steamPricesClient.GetItemMarketListingsWithHistoryRaw(appId, marketHashName, token));

        if (!steamServiceResult.TryPickResult(out var responseHtml, out var errors)) return errors;

        if (responseHtml is null) return null as GameItemMarketHistoryChartResponse;

        var startOfJsArraySplit = responseHtml.Split("var line1=");
        if (startOfJsArraySplit.Length < 2) return new NotFound();
        var historyChartInJsArrayForm = startOfJsArraySplit[1].Split(";")[0];

        IEnumerable<GameItemMarketHistoryChartPointResponse> historyChartPoints =
            ParseHistoryChartPointsFromJsArray(historyChartInJsArrayForm);
        return new GameItemMarketHistoryChartResponse(historyChartPoints);


        static IEnumerable<GameItemMarketHistoryChartPointResponse> ParseHistoryChartPointsFromJsArray(string jsArray)
        {
            var trimExtraBracesThenSplitInputToArrayOfArrays = jsArray.Trim('[', ']').Split("],[");

            IEnumerable<string[]> splitEachArrayToElementsThenTrimQuotes = trimExtraBracesThenSplitInputToArrayOfArrays
                .Select(tuple => tuple.Split(",")
                    .Select(tupleElement => tupleElement.Trim('\"')).ToArray());

            IEnumerable<GameItemMarketHistoryChartPointResponse> historyChartPoints =
                splitEachArrayToElementsThenTrimQuotes
                    .Select(elements =>
                        new GameItemMarketHistoryChartPointResponse(elements[0], elements[1], elements[2]));
            return historyChartPoints;
        }
    }
}

// var parallelAttemptsCount = 3;
// if (parallelAttemptsCount < 1)
//     throw new ArgumentOutOfRangeException(nameof(parallelAttemptsCount), "Value should be greater than zero");
// if (parallelAttemptsCount == 1) return await GetItemLowestMarketPriceUsd(appId, marketHashName, token);