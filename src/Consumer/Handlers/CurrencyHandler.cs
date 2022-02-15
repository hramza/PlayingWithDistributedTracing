namespace Consumer.Handlers
{
    public class CurrencyHandler : ICurrencyHandler
    {
        private readonly ICurrencyApiProvider _currencyApiProvider;
        private readonly ISearchEngineLoader _searchEngineLoader;

        public CurrencyHandler(ICurrencyApiProvider currencyApiProvider, ISearchEngineLoader searchEngineLoader)
        {
            ArgumentNullException.ThrowIfNull(currencyApiProvider, nameof(currencyApiProvider));
            ArgumentNullException.ThrowIfNull(searchEngineLoader, nameof(searchEngineLoader));

            _currencyApiProvider = currencyApiProvider;
            _searchEngineLoader = searchEngineLoader;
        }

        public async Task HandleAsync(CurrencyRequest currencyRequest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(currencyRequest, nameof(currencyRequest));

            // call currency API
            var response = await _currencyApiProvider.GetCurrencyDataAsync(currencyRequest);
            if (response is null) return;

            // Index in elastic
            await _searchEngineLoader.IndexAsync(response, cancellationToken);
        }
    }
}
