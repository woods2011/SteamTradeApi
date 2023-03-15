using System.ComponentModel.DataAnnotations;

namespace SteamClientTestPolygonWebApi.Contracts.Requests;

// ToDo: Переделать в command

public class LoadSteamInventoryCommand
{
    [Required]
    public int AppId { get; init; }

    [Required]
    public long Steam64Id { get; init; }

    [Range(1, 5000)]
    public int MaxCount { get; init; } = 5000;
}