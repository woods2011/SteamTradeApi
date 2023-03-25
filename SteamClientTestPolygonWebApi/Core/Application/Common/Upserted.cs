namespace SteamClientTestPolygonWebApi.Core.Application.Common;

public record struct Upserted<T>(T Entity, bool IsNewlyCreated);