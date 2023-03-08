using System.Text.Json.Serialization;

namespace SteamClientTestPolygonWebApi.Contracts.External;

public class SteamSdkInventoryResponseOld
{
    [JsonPropertyName("rgInventory")]
    public Dictionary<string, SteamSdkAssetResponseOld> RgInventory { get; init; }

    [JsonPropertyName("rgDescriptions")]
    public Dictionary<string, SteamSdkDescriptionResponseOld> RgDescriptions { get; init; }

    public bool? Success { get; init; }

    [JsonPropertyName("rgCurrency")]
    public IEnumerable<string> RgCurrency { get; init; }

    public bool? More { get; init; }

    public int? MoreStart { get; init; }
}

public class SteamSdkAssetResponseOld
{
    public string? Id { get; init; }

    public string? ClassId { get; init; }

    public string? InstanceId { get; init; }

    public string? Amount { get; init; }

    public int? HideInChina { get; init; }

    public int? Pos { get; init; }

    // "26758583257": {
    //     "id": "26758583257",
    //     "classid": "5061407183",
    //     "instanceid": "5211613952",
    //     "amount": "1",
    //     "hide_in_china": 0,
    //     "pos": 1
    // },
}

public class SteamSdkDescriptionResponseOld
{
    public string? AppId { get; init; }

    public string? ClassId { get; init; }

    public string? InstanceId { get; init; }

    public string? IconUrl { get; init; }

    public string? IconUrlLarge { get; init; }

    public string? IconDragUrl { get; init; }

    public string? Name { get; init; }

    public string? MarketHashName { get; init; }

    public string? MarketName { get; init; }

    public string? NameColor { get; init; }

    public string? BackgroundColor { get; init; }

    public string? Type { get; init; }

    public int? Tradable { get; init; }

    public int? Marketable { get; init; }

    public int? Commodity { get; init; }

    public string? MarketTradableRestriction { get; init; }

    public string? MarketMarketableRestriction { get; init; }

    public string? CacheExpiration { get; init; }
    
    [JsonConverter(typeof(EmptyStringToEmptyArrayConverter<SteamSdkNestedDescriptionResponseOld>))]
    public IEnumerable<SteamSdkNestedDescriptionResponseOld>? Descriptions { get; init; }

    [JsonConverter(typeof(EmptyStringToEmptyArrayConverter<SteamSdkNestedDescriptionResponseOld>))]
    public IEnumerable<SteamSdkNestedDescriptionResponseOld>? OwnerDescriptions { get; init; }

    public SteamSdkItemTagResponseOld[]? Tags { get; init; }

    // "5061407183_5211613952": {
    //      "appid": "570",
    //      "classid": "5061407183",
    //      "instanceid": "5211613952",
    //      "icon_url": "-9a81dlWLwJ2UUG",
    //      "icon_url_large": "-9a81dlWLwJ2UU",
    //      "icon_drag_url": "",
    //      "name": "The Abscesserator",
    //      "market_hash_name": "The Abscesserator",
    //      "market_name": "The Abscesserator",
    //      "name_color": "D2D2D2",
    //      "background_color": "",
    //      "type": "Immortal Wearable",
    //      "tradable": 0,
    //      "marketable": 1,
    //      "commodity": 0,
    //      "market_tradable_restriction": "7",
    //      "market_marketable_restriction": "0",
    //      "cache_expiration": "2023-03-04T16:00:00Z",
    //      "descriptions": [
    //      	{
    //      		"type": "html",
    //      		"value": "Used By: Pudge"
    //      	}
    //      ],
    //      "owner_descriptions": [
    //      	{
    //      		"value": "\nOn Trade Cooldown Until: Sat Mar  4 08:00:00 2023"
    //      	}
    //      ],
    //      "tags": [
    //          {
    //              "internal_name": "unique",
    //              "name": "Standard",
    //              "category": "Quality",
    //              "color": "D2D2D2",
    //              "category_name": "Quality"
    //          },
    //      ]   
    // }
    
    // [JsonConstructor]
    // public SteamSdkDescriptionResponseOld(JsonElement? descriptions)
    // {
    //     var descriptionsReal = descriptions is { ValueKind: JsonValueKind.Array }
    //         ? JsonSerializer.Deserialize<IEnumerable<SteamSdkNestedDescriptionResponseOld>>
    //             (descriptions.Value.ToString(), SteamApiJsonSettings.Default)
    //         : Array.Empty<SteamSdkNestedDescriptionResponseOld>();
    //     DescriptionsReal = descriptionsReal ?? Array.Empty<SteamSdkNestedDescriptionResponseOld>();
    // }
}

public class SteamSdkNestedDescriptionResponseOld
{
    public string Type { get; init; }

    public string Value { get; init; }

    public string? Color { get; init; }

    // {
    //     "type": "html",
    //     "value": "The International 2015",
    //     "color": "99ccff"
    // },
}

public class SteamSdkItemTagResponseOld
{
    public string? InternalName { get; init; }

    public string? Name { get; init; }

    public string? Category { get; init; }

    public string? Color { get; init; }

    public string? CategoryName { get; init; }

    // {
    //  "internal_name": "unique",
    //  "name": "Standard",
    //  "category": "Quality",
    //  "color": "D2D2D2",
    //  "category_name": "Quality"
    // }
}


