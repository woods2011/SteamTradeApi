namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Truncate string to a certain length
    /// </summary>
    /// <returns>Truncated string</returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (String.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}