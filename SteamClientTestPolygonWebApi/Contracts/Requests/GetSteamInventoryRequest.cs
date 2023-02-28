using System.ComponentModel.DataAnnotations;

namespace SteamClientTestPolygonWebApi.Contracts.Requests;

public class GetSteamInventoryRequest
{
    [Required] public long Steam64Id { get; init; }

    [Range(1, 5000)] public int MaxCount { get; init; } = 5000;
}