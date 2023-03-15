using SteamClientTestPolygonWebApi.Contracts.External;

namespace SteamClientTestPolygonWebApi.Application.Utils.TradeCooldownParsers;

public class GeneralCooldownInfoScraper
{
    public string? TryFindCooldownString(SteamSdkDescriptionResponse itemDescription) =>
        itemDescription.Descriptions?.LastOrDefault(od => od.Value.Length is > 40 and < 70)?.Value;
}