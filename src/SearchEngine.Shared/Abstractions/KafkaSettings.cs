namespace SearchEngine.Shared.Abstractions
{
    public class KafkaSettings
    {
        public string? Server { get; init; }
        public string? Topic { get; init; }
    }
}
