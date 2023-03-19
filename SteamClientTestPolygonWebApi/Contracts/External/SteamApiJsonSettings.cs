using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;

namespace SteamClientTestPolygonWebApi.Contracts.External
{
    public class SteamNamingPolicy : JsonNamingPolicy
    {
        private readonly SnakeCaseNamingStrategy _newtonsoftSnakeCaseNamingStrategy = new();

        public static SteamNamingPolicy Instance { get; } = new();

        private SteamNamingPolicy() { }

        public override string ConvertName(string name)
        {
            name = Regex.Replace(name, @"(.??)Id([^a-z]|$)", "$1id$2", RegexOptions.Compiled);
            return _newtonsoftSnakeCaseNamingStrategy.GetPropertyName(name, false);
        }

        private static string ConvertWordToLowerInPascalCase(string input, string word) =>
            Regex.Replace(input, $@"(.??){word}([^a-z]|$)", $"$1{word.ToLower()}$2", RegexOptions.Compiled);
    }

    public static class SteamApiJsonSettings
    {
        public static JsonSerializerOptions Default => 
            new() { PropertyNamingPolicy = SteamNamingPolicy.Instance };
    }

    public class EmptyStringToEmptyArrayConverter<T> : JsonConverter<IEnumerable<T>>
    {
        public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                return Array.Empty<T>();

            var list = new List<T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) return list;
                var item = JsonSerializer.Deserialize<T>(ref reader, options);
                if (item is not null) list.Add(item);
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value,
            JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);

        // public override void Write(Utf8JsonWriter writer, IEnumerable<T> value,
        //     JsonSerializerOptions options)
        // {
        //     if (value.Any())
        //         JsonSerializer.Serialize(writer, value, options);
        //     else
        //         JsonSerializer.Serialize(writer, "", options);
        // }
    }
}