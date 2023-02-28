namespace SteamClientTestPolygonWebApi.Contracts.External;

public class ExternalSteamInventoryResponse
{
    public List<ExternalItemAssetResponse> Assets { get; }

    public List<ExternalItemDescriptionResponse> Descriptions { get; }

    public int TotalInventoryCount { get; }

    public ExternalSteamInventoryResponse(List<ExternalItemAssetResponse> assets,
        List<ExternalItemDescriptionResponse> descriptions, int totalInventoryCount)
    {
        Assets = assets;
        Descriptions = descriptions;
        TotalInventoryCount = totalInventoryCount;
    }
}

public class ExternalItemAssetResponse
{
    public string AssetId { get; }

    public string ClassId { get; }

    public string InstanceId { get; }

    public ExternalItemAssetResponse(string assetId, string classId, string instanceId)
    {
        AssetId = assetId;
        ClassId = classId;
        InstanceId = instanceId;
    }
}

public class ExternalItemDescriptionResponse
{
    public string ClassId { get; }

    public string InstanceId { get; }

    public int Tradable { get; }

    public ExternalItemDescriptionResponse(string classId, string instanceId, int tradable)
    {
        ClassId = classId;
        InstanceId = instanceId;
        Tradable = tradable;
    }
}