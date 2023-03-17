namespace SteamClientTestPolygonWebApi.Application.Common;

public record struct Upserted<T>(T Entity, bool IsNewlyCreated);