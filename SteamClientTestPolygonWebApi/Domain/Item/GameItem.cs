namespace SteamClientTestPolygonWebApi.Domain.Item;

public class GameItem
{
    public int AppId { get; private set; }
    public string MarketHashName { get; private set; }
    public string IconUrl { get; private set; }
    public string ClassId { get; private set; }

    public PriceInfo? PriceInfo { get; set; } = null;
    

    private GameItem(
        int appId,
        string marketHashName,
        string iconUrl,
        string classId)
    {
        AppId = appId;
        MarketHashName = marketHashName;
        IconUrl = iconUrl;
        ClassId = classId;
    }

    public static GameItem Create(int appId, string marketHashName, string iconUrl, string classId)
        => new(appId, marketHashName, iconUrl, classId);


#pragma warning disable CS8618
    private GameItem() { } // For EF Core
#pragma warning restore CS8618
}

// AppId + ItemMarketHashName = GameItemId