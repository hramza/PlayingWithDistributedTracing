namespace SearchEngine.Shared.Abstractions
{
    public interface IKafkaProducer<T> where T : class
    {
        Task ProduceAsync(T record);
    }
}
