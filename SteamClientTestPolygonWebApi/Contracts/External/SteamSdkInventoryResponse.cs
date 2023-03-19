namespace SteamClientTestPolygonWebApi.Contracts.External;

public record SteamSdkInventoryResponse(
    IReadOnlyList<SteamSdkAssetResponse> Assets,
    IReadOnlyList<SteamSdkDescriptionResponse> Descriptions,
    int TotalInventoryCount);

public record SteamSdkAssetResponse(
    int AppId,
    string ContextId,
    string AssetId,
    string ClassId,
    string InstanceId,
    string Amount);

public record SteamSdkDescriptionResponse(
    int AppId,
    string ClassId,
    string InstanceId,
    string IconUrl,
    IReadOnlyList<SteamSdkNestedDescriptionResponse>? Descriptions,
    int Tradable,
    string MarketHashName,
    int Marketable,
    IReadOnlyList<SteamSdkItemTagResponse> Tags) { }

public record SteamSdkNestedDescriptionResponse(string? Type, string Value);

public record SteamSdkItemTagResponse(string Category, string InternalName);


// {
// "appid": 570,
// "contextid": "2",
// "assetid": "10296117243",
// "classid": "2143278740",
// "instanceid": "1625510296",
// "amount": "1"
// },


//   "appid": 570,
//   "classid": "230141732",
//   "instanceid": "0",
//   "currency": 0,
//   "background_color": "",
//   "icon_url": "-9a81dlWLwJ2UUGcVs_nsVtzdOEdtWwKGZZLQHTxDZ7I56KW1Zwwo4NUX4oFJZEHLbXK9QlSPcUioRpTWEPdeOW-xM7AQFR6agBWpLGaPBVh2ufATipH7cy5ms-PluX_DKzDl2JF4Ppmj-jR-oK731Xk-Ec_MG32JYeTcwI5NwvRqFLsyeq70Me46ZSdnHJruHYgtHeMlgv330_n7zQhOg",
//   "icon_url_large": "-9a81dlWLwJ2UUGcVs_nsVtzdOEdtWwKGZZLQHTxDZ7I56KW1Zwwo4NUX4oFJZEHLbXK9QlSPcUioRpTWEPdeOW-xM7AQFR6agBWpLGaPBVh2ufATipH7cy5ms-PluX_DKzDl2JF4Ppmj-jR-oLKhFWmrBZyNjrwdoOdcwU9aA3T-1e8xb_rgpO5vs7MnXQyuSF27XeMmBG1gUxMb_sv26Iim-e6FA",
//   "descriptions": [
//     {
//       "type": "html",
//       "value": "Используется: Phantom Assassin"
//     },
//     {
//       "type": "html",
//       "value": " "
//     },
//     {
//       "type": "html",
//       "value": "Dark Wraith",
//       "color": "9da1a9"
//     },
//     {
//       "type": "html",
//       "value": "Cloak of the Dark Wraith",
//       "color": "6c7075"
//     },
//     {
//       "type": "html",
//       "value": "Girdle of the Dark Wraith",
//       "color": "6c7075"
//     },
//     {
//       "type": "html",
//       "value": "Helm of the Dark Wraith",
//       "color": "6c7075"
//     },
//     {
//       "type": "html",
//       "value": "Guard of the Dark Wraith",
//       "color": "6c7075"
//     },
//     {
//       "type": "html",
//       "value": "Blade of the Dark Wraith",
//       "color": "6c7075"
//     },
//     {
//       "type": "html",
//       "value": "Напоминание о том, что внимание Тёмного духа равносильно смерти."
//     },
//     {
//       "type": "html",
//       "value": " "
//     },
//     {
//       "type": "html",
//       "value": "(Нельзя передавать)"
//     }
//   ],
//   "tradable": 0,
//   "name": "Blade of the Dark Wraith",
//   "name_color": "D2D2D2",
//   "type": "Аксессуар, Uncommon",
//   "market_name": "Blade of the Dark Wraith",
//   "market_hash_name": "Blade of the Dark Wraith",
//   "commodity": 0,
//   "market_tradable_restriction": 7,
//   "market_marketable_restriction": 0,
//   "marketable": 0,
//   "tags": [
//     {
//       "category": "Quality",
//       "internal_name": "unique",
//       "localized_category_name": "Качество",
//       "localized_tag_name": "Обычная",
//       "color": "D2D2D2"
//     },
//     {
//       "category": "Rarity",
//       "internal_name": "Rarity_Uncommon",
//       "localized_category_name": "Редкость",
//       "localized_tag_name": "Uncommon",
//       "color": "5e98d9"
//     },
//     {
//       "category": "Type",
//       "internal_name": "wearable",
//       "localized_category_name": "Тип",
//       "localized_tag_name": "Украшение"
//     },
//     {
//       "category": "Slot",
//       "internal_name": "weapon",
//       "localized_category_name": "Ячейка",
//       "localized_tag_name": "Оружие"
//     },
//     {
//       "category": "Hero",
//       "internal_name": "npc_dota_hero_phantom_assassin",
//       "localized_category_name": "Герой",
//       "localized_tag_name": "Phantom Assassin"
//     }
//   ]
// }