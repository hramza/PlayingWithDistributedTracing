using System.Diagnostics;

namespace Consumer.Providers
{
    public class SearchEngineLoader : ISearchEngineLoader
    {
        private static readonly ActivitySource ActivitySource = new("elastic");
        
        private readonly ElasticClient _client;
        private readonly ElasticSearchConfiguration _elasticConfiguration;
        private readonly string _indexName;

        public SearchEngineLoader(ElasticClient elasticClient, ElasticSearchConfiguration elasticConfiguration)
        {
            ArgumentNullException.ThrowIfNull(elasticClient, nameof(elasticClient));
            ArgumentNullException.ThrowIfNull(elasticConfiguration, nameof(elasticConfiguration));

            _client = elasticClient;
            _elasticConfiguration = elasticConfiguration;
            _indexName = _elasticConfiguration.IndexName!;
        }

        public async Task CreateIndexAsync()
        {
            var response = await _client.Indices.ExistsAsync(_indexName);

            if (!response.Exists)
            {
                var indexResponse = await _client.Indices.CreateAsync(_indexName, i => i
                    .Map(m => m
                        .AutoMap<CurrencyData>()
                        .Properties<CurrencyData>(p => p.Keyword(k => k.Name(f => f.Pair))))
                    .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0)));

                if (!indexResponse.IsValid || indexResponse.Acknowledged is false)
                    throw new Exception("Something goes wrong while creating index !");
            }
        }

        public async Task IndexAsync(CurrencyData document, CancellationToken cancellationToken)
        {
            await _client.IndexDocumentAsync(document, cancellationToken);

            using var activity = ActivitySource.StartActivity("data indexed", ActivityKind.Internal);
            activity?.AddEvent(new ActivityEvent("Index currency response", tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Pair", document.Pair) })));
            activity?.SetTag("otel.status_code", "OK");
            activity?.SetTag("otel.status_description", "Indexed successfully");
        }
    }
}
