namespace SteamClientTestPolygonWebApi.Contracts.External;

public record SteamSdkItemPriceResponse(string LowestPrice, string? Volume, string? MedianPrice);


// public class SteamSdkItemPriceResponse
// {
//     public SteamSdkItemPriceResponse(string LowestPrice, string? Volume, string? MedianPrice)
//     {
//         this.LowestPrice = LowestPrice ?? throw new ArgumentNullException(nameof(LowestPrice));
//         this.Volume = Volume;
//         this.MedianPrice = MedianPrice;
//     }
//     public string LowestPrice { get; }
//     public string? Volume { get; }
//     public string? MedianPrice { get; }
// }