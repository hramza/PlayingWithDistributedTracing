namespace Consumer.Abstractions
{
    public interface ISearchEngineLoader
    {
        Task IndexAsync(CurrencyData document, CancellationToken cancellationToken);

        Task CreateIndexAsync();
    }
}
