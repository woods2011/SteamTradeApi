namespace SteamClientTestPolygonWebApi.Domain.Item;

public class GameItem
{
    public int AppId { get; private set; }
    public string MarketHashName { get; private set; }
    public string IconUrl { get; private set; }
    public string ClassId { get; private set; }

    public PriceInfo? PriceInfo { get; private set; }


    private GameItem(
        int appId,
        string marketHashName,
        string iconUrl,
        string classId,
        PriceInfo? priceInfo = null)
    {
        AppId = appId;
        MarketHashName = marketHashName;
        IconUrl = iconUrl;
        ClassId = classId;
        PriceInfo = priceInfo;
    }

    public static GameItem Create(int appId, string marketHashName, string iconUrl, string classId)
        => new(appId, marketHashName, iconUrl, classId);

    public void UpdatePriceInfo(PriceInfo priceInfo)
    {
        if (PriceInfo is null) PriceInfo = priceInfo;
        else PriceInfo.Update(priceInfo);
    }


#pragma warning disable CS8618
    private GameItem() { } // For EF Core
#pragma warning restore CS8618
}

// AppId + ItemMarketHashName = GameItemId