using System.Globalization;
using System.Text.RegularExpressions;
using SteamClientTestPolygonWebApi.Application.Common;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Controllers;

namespace SteamClientTestPolygonWebApi.Application.Utils.TradeCooldownParsers;

public interface ITradeCooldownParserFactory
{
    ITradeCooldownParser Create(int appId);
}

public class TradeCooldownParserFactory : ITradeCooldownParserFactory
{
    private readonly IReadOnlyDictionary<int, ITradeCooldownParser> _parsersMap;
    private readonly FallbackCooldownParser _fallbackCooldownParser;

    public TradeCooldownParserFactory(IDateTimeProvider dateTimeProvider)
    {
        _fallbackCooldownParser = new FallbackCooldownParser(dateTimeProvider);

        var parsers = new List<IAppSpecificTradeCooldownParser>
        {
            new Dota2TradeCooldownParser(),
            // new Tf2TradeCooldownParser(_dateTimeProvider),
            // new CsgoTradeCooldownParser(_dateTimeProvider),
        };

        // Нужен Cast т.к. IReadOnlyDictionary не ковариантен по TValue
        _parsersMap = parsers.ToDictionary(parser => parser.AppId, parser => (ITradeCooldownParser) parser);
    }

    public ITradeCooldownParser Create(int appId)
    {
        return _parsersMap.TryGetValue(appId, out var parser)
            ? parser
            : _fallbackCooldownParser;
    }
}