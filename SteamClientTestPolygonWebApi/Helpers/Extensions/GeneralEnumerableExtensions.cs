namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class GeneralEnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence) => sequence.OfType<T>();
}