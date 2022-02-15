using Confluent.Kafka;

namespace SearchEngine.Shared.Abstractions
{
    public interface IKafkaConsumer<T> where T : class
    {
        Task Subscribe(Func<ConsumeResult<Guid, T>, Task> callback, CancellationToken cancellationToken);
    }
}
