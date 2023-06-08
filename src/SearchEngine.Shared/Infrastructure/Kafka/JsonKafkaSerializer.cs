using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace SearchEngine.Shared.Infrastructure.Kafka
{
    public sealed class JsonKafkaSerializer<T> : ISerializer<T>, IDeserializer<T> where T : class
    {
        private JsonKafkaSerializer() { }

        public static JsonKafkaSerializer<T> Instance { get; } = new JsonKafkaSerializer<T>();

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>  JsonSerializer.Deserialize<T>(data)!;

        public byte[] Serialize(T data, SerializationContext context) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
    }
}
