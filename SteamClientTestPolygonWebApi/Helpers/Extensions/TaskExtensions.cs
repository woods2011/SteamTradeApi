namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class TaskExtensions
{
    public static Task<T[]> WhenAllAsync<T>(this IEnumerable<Task<T>> values) => Task.WhenAll(values);
}