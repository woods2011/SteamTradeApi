using SteamClientTestPolygonWebApi.Application.Utils.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Contracts.Requests;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Domain.GameItemAggregate;

namespace SteamClientTestPolygonWebApi.Application.Mappers.ManualToDomain;

public static class MapExternalInventoryRepresentation
{
    public static GameInventoryAsset MapToGameInventoryAsset(
        this (SteamSdkAssetResponse asset, SteamSdkDescriptionResponse descr, LoadSteamInventoryCommand req) source,
        ITradeCooldownParser tradeCooldownParser)
    {
        var (asset, itemDescription, request) = source;

        var isTradable = itemDescription.Tradable is 1;
        var isMarketable = itemDescription.Marketable is 1;
        var tradeCooldownUntilUtc = isTradable || !isMarketable
            ? null
            : tradeCooldownParser.TryParseItemDescription(itemDescription);

        return GameInventoryAsset.Create(
            assetId: asset.AssetId,
            appId: asset.AppId,
            itemMarketHashName: itemDescription.MarketHashName,
            ownerSteam64Id: request.Steam64Id.ToString(),
            isTradable: isTradable,
            tradeCooldownUntilUtc: tradeCooldownUntilUtc,
            isMarketable: isMarketable,
            instanceId: asset.InstanceId);
    }

    public static GameInventory MapToGameInventory(
        this SteamSdkInventoryResponse inventory, LoadSteamInventoryCommand command,
        DateTime nowUtc, ITradeCooldownParser tradeCooldownParser)
    {
        var assets = MapToGameInventoryAssets(inventory, command, tradeCooldownParser);

        return GameInventory.Create(
            appId: command.AppId,
            ownerSteam64Id: command.Steam64Id.ToString(),
            lastUpdateDateTimeUtc: nowUtc,
            assets: assets);
    }
    
    public static List<GameInventoryAsset> MapToGameInventoryAssets(
        this SteamSdkInventoryResponse inventory, LoadSteamInventoryCommand command,
        ITradeCooldownParser tradeCooldownParser)
    {
        var assets = inventory.Assets.Join(inventory.Descriptions,
                asset => (asset.ClassId, asset.InstanceId),
                description => (description.ClassId, description.InstanceId),
                (asset, description) =>
                    (asset, description, command).MapToGameInventoryAsset(tradeCooldownParser))
            .ToList();

        return assets;
    }

    public static GameItem MapToGameItem(this SteamSdkDescriptionResponse descriptionResponse) =>
        GameItem.Create(
            appId: descriptionResponse.AppId,
            marketHashName: descriptionResponse.MarketHashName,
            iconUrl: descriptionResponse.IconUrl,
            classId: descriptionResponse.ClassId);
}