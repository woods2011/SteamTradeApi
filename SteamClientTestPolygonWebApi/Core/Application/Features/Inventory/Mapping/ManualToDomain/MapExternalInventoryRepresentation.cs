using SteamClientTestPolygonWebApi.Contracts.External;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Commands;
using SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.TradeCooldownParsers;
using SteamClientTestPolygonWebApi.Core.Domain.GameInventoryAggregate;
using SteamClientTestPolygonWebApi.Core.Domain.GameInventoryAggregate.Entities;
using SteamClientTestPolygonWebApi.Core.Domain.Item;

namespace SteamClientTestPolygonWebApi.Core.Application.Features.Inventory.Mapping.ManualToDomain;

public static class MapExternalInventoryRepresentation
{
    public static GameInventoryAsset MapToGameInventoryAsset(
        this (SteamSdkAssetResponse asset, SteamSdkDescriptionResponse descr, long steam64Id) source,
        ITradeCooldownParser tradeCooldownParser)
    {
        var (asset, itemDescription, steam64Id) = source;

        var isTradable = itemDescription.Tradable is 1;
        var isMarketable = itemDescription.Marketable is 1;
        DateTime? tradeCooldownUntilUtc = isTradable || !isMarketable
            ? null
            : tradeCooldownParser.TryParseItemDescription(itemDescription);

        return GameInventoryAsset.Create(
            assetId: asset.AssetId,
            appId: asset.AppId,
            ownerSteam64Id: steam64Id.ToString(),
            itemMarketHashName: itemDescription.MarketHashName,
            isTradable: isTradable,
            tradeCooldownUntilUtc: tradeCooldownUntilUtc,
            isMarketable: isMarketable,
            instanceId: asset.InstanceId);
    }

    public static GameInventory MapToGameInventory(
        this SteamSdkInventoryResponse inventory, LoadInventoryCommand request,
        DateTime nowUtc, ITradeCooldownParser tradeCooldownParser)
    {
        List<GameInventoryAsset> assets = MapToGameInventoryAssets(inventory, request.Steam64Id, tradeCooldownParser);

        return GameInventory.Create(
            appId: request.AppId,
            ownerSteam64Id: request.Steam64Id.ToString(),
            lastUpdateDateTimeUtc: nowUtc,
            assets: assets);
    }

    public static List<GameInventoryAsset> MapToGameInventoryAssets(
        this SteamSdkInventoryResponse inventory, long steam64Id, ITradeCooldownParser tradeCooldownParser)
    {
        List<GameInventoryAsset> assets = inventory.Assets.Join(inventory.Descriptions,
                asset => (asset.ClassId, asset.InstanceId),
                description => (description.ClassId, description.InstanceId),
                (asset, description) =>
                    (asset, description, steam64Id).MapToGameInventoryAsset(tradeCooldownParser)).ToList();

        return assets;
    }

    public static GameItem MapToGameItem(this SteamSdkDescriptionResponse descriptionResponse) =>
        GameItem.Create(
            appId: descriptionResponse.AppId,
            marketHashName: descriptionResponse.MarketHashName,
            iconUrl: descriptionResponse.IconUrl,
            classId: descriptionResponse.ClassId);
}