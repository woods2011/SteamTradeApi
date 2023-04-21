using System.Globalization;
using System.Text.RegularExpressions;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Core.Application.Common;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.TradeCooldownParsers;

public interface ITradeCooldownParser
{
    DateTime? TryParseItemDescription(SteamSdkDescriptionResponse itemDescription);
}

public interface IAppSpecificTradeCooldownParser : ITradeCooldownParser
{
    int AppId { get; }
}

public class FallbackCooldownParser : ITradeCooldownParser
{
    // private readonly GeneralCooldownStringScraper _generalCooldownStringScraper = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public FallbackCooldownParser(IDateTimeProvider dateTimeProvider) => _dateTimeProvider = dateTimeProvider;

    public DateTime? TryParseItemDescription(SteamSdkDescriptionResponse itemDescription) =>
        _dateTimeProvider.UtcNow.AddDays(7);
}

public class Dota2TradeCooldownParser : IAppSpecificTradeCooldownParser
{
    private readonly GeneralCooldownInfoScraper _generalCooldownInfoScraper = new();

    public DateTime? TryParseItemDescription(SteamSdkDescriptionResponse itemDescription) =>
        _generalCooldownInfoScraper.TryFindCooldownString(itemDescription) switch
        {
            null => null,
            var tradeCooldownString => TryParseDota2TradeCooldown(tradeCooldownString)
        };

    static DateTime? TryParseDota2TradeCooldown(string tradeCooldownString)
    {
        ReadOnlySpan<char> removedPrefixAndTrimmed = RemovePrefixAndTrim(tradeCooldownString);
        var preparedStr = ReplaceDoubleSpaces(removedPrefixAndTrimmed.ToString());

        var (dateFormat, culture, none) = ("MMM d, yyyy (H:mm:ss)", CultureInfo.InvariantCulture, DateTimeStyles.None);
        if (!DateTime.TryParseExact(preparedStr, dateFormat, culture, none, out DateTime expirationDateNoOffset))
            return null;

        var expirationDatePacificTimeZone = new DateTimeOffset(expirationDateNoOffset, TimeSpan.FromHours(-8));
        return expirationDatePacificTimeZone.UtcDateTime;

        // ----------------- Local functions -----------------
        static ReadOnlySpan<char> RemovePrefixAndTrim
            (ReadOnlySpan<char> input) => input.Slice(26).Trim();

        static string ReplaceDoubleSpaces
            (string input) => Regex.Replace(input, " {2,}", " ", RegexOptions.Compiled);
    }

    public int AppId => (int) AppIds.Dota2;
}

public class GeneralCooldownInfoScraper
{
    public string? TryFindCooldownString(SteamSdkDescriptionResponse itemDescription) =>
        itemDescription.Descriptions?.LastOrDefault(od => od.Value.Length is > 40 and < 70)?.Value;
}

public enum AppIds
{
    Tf2 = 440,
    Dota2 = 570,
    Csgo = 730,
    // Rust = 252490,
    // Pubg = 578080
}


// /// <summary>
// /// Try parse trade cooldown from item description for untradable items 
// /// </summary>
// /// <param name="itemDescription"></param>
// /// <returns> NULL if item is not tradable or can't parse trade cooldown from description;<br/>
// /// otherwise return trade cooldown date time in UTC</returns>
// private DateTime? ParseDescriptionForTradeCooldown(SteamSdkDescriptionResponse itemDescription)

// DateTime.ParseExact(preparedString, "ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture);