using System.ComponentModel.DataAnnotations;

namespace SteamClientTestPolygonWebApi.Contracts.Requests;

public class GetSteamInventoryQuery
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }
}