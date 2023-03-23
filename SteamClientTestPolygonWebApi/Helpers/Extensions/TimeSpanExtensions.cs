namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class TimeSpanExtensions
{
    public static TimeSpan PlusJitter(this TimeSpan time, double multiplier = 0.1, Random? random = null)
    {
        random ??= Random.Shared;
        var jitter = 2 * random.NextDouble() * multiplier;
        return time * (1 + jitter);
    }
}