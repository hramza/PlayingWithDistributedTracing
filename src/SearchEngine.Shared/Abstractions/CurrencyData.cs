using System.Text.Json.Serialization;

namespace SearchEngine.Shared.Abstractions
{
    public record CurrencyData(
        [property: JsonPropertyName("pair")] string Pair,
        [property: JsonPropertyName("low")] double LowPrice,
        [property: JsonPropertyName("high")] double HighPrice);
}
