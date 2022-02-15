using System.Text.Json;
using System.Text.Json.Serialization;

namespace SearchEngine.Shared.Serializers
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        static JsonHelper()
        {
            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public static T? Deserialize<T>(string jsonString) => JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);

        public static string Serialize(object @object) => JsonSerializer.Serialize(@object, JsonSerializerOptions);
    }
}
