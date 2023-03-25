namespace SteamClientTestPolygonWebApi.Core.Domain.Item;

public class PriceInfo
{
    public decimal LowestMarketPriceUsd { get; private set; }

    public decimal? MedianMarketPriceUsd { get; private set; }

    public DateTime LastUpdateUtc { get; private set; }


    private PriceInfo(decimal lowestMarketPriceUsd, decimal? medianMarketPriceUsd, DateTime lastUpdateUtc)
    {
        LowestMarketPriceUsd = lowestMarketPriceUsd;
        MedianMarketPriceUsd = medianMarketPriceUsd;
        LastUpdateUtc = lastUpdateUtc;
    }

    public static PriceInfo Create(decimal lowestMarketPriceUsd, decimal? medianMarketPriceUsd, DateTime lastUpdateUtc)
        => new(lowestMarketPriceUsd, medianMarketPriceUsd, lastUpdateUtc);


    public void Update(PriceInfo newPriceInfo)
    {
        LowestMarketPriceUsd = newPriceInfo.LowestMarketPriceUsd;
        LastUpdateUtc = newPriceInfo.LastUpdateUtc;

        var newMedian = newPriceInfo.MedianMarketPriceUsd;
        if (newMedian is null) return;

        if (MedianMarketPriceUsd is null)
        {
            MedianMarketPriceUsd = newMedian;
            return;
        }

        MedianMarketPriceUsd = (MedianMarketPriceUsd + newMedian) / 2;
    }
}

// ValueObject