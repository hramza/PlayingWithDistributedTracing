using Confluent.Kafka;

namespace SearchEngine.Shared.Infrastructure.Kafka
{
    public class GuidKafkaSerializer : ISerializer<Guid>, IDeserializer<Guid>
    {
        private GuidKafkaSerializer() { }

        public static GuidKafkaSerializer Instance { get; } = new();

        public byte[] Serialize(Guid data, SerializationContext context) => data.ToByteArray();

        public Guid Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) => new(data);
    }
}
