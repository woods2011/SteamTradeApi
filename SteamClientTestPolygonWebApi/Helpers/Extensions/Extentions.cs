using System.Collections;

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

/// <summary>
/// Extensions to avoid possible multiple enumeration of <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableMaterializeExtensions
{
    /// <summary>
    /// Materialize <see cref="IEnumerable{T}"/> into <see cref="IReadOnlyCollection{T}"/> to avoid possible multiple enumeration
    /// </summary>
    /// <returns>Materialized collection</returns>
    /// <exception cref="ArgumentNullException"/>
    public static IReadOnlyCollection<T> MaterializeToIReadOnlyCollection<T>(this IEnumerable<T> source) =>
        source switch
        {
            IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection,
            ICollection<T> collection => new ToIReadOnlyCollectionAdapter<T>(collection),
            null => throw new ArgumentNullException(nameof(source)),
            _ => source.ToList()
        };

    /// <summary>
    /// Materialize <see cref="IEnumerable{T}"/> into <see cref="IReadOnlyList{T}"/> to avoid possible multiple enumeration
    /// </summary>
    /// <returns>Materialized collection</returns>
    /// <exception cref="ArgumentNullException"/>
    public static IReadOnlyList<T> MaterializeToIReadOnlyList<T>(this IEnumerable<T> source) =>
        source switch
        {
            IReadOnlyList<T> readOnlyList => readOnlyList,
            IList<T> list => new ToIReadOnlyListAdapter<T>(list), // ... => new ReadOnlyCollection<T>(list)
            null => throw new ArgumentNullException(nameof(source)),
            _ => source.ToList()
        };

    private class ToIReadOnlyCollectionAdapter<T> : IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _source;

        public int Count => _source.Count;

        public ToIReadOnlyCollectionAdapter(ICollection<T> source) => _source = source;

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private class ToIReadOnlyListAdapter<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _source;

        public int Count => _source.Count;

        public ToIReadOnlyListAdapter(IList<T> source) => _source = source;

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T this[int index] => _source[index];
    }
}