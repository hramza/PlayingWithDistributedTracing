const string SourceName = "Consumer";
const string elasticSourceName = "elastic";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddOpenTelemetryTracing(traceBuilder =>
        {
            traceBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SourceName))
                .AddSource(SourceName)
                .AddSource(elasticSourceName)
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("AppSettings:OtelEndpoint"));
                });
        });

        services.Configure<KafkaSettings>(options => builder.Configuration.GetSection(nameof(KafkaSettings)).Bind(options));
        services.AddSingleton(typeof(IKafkaConsumer<>), typeof(KafkaConsumer<>));

        ElasticSearchConfiguration elasticConfiguration = new();
        builder.Configuration.GetSection(nameof(ElasticSearchConfiguration)).Bind(elasticConfiguration);
        services.AddSingleton<ISearchEngineLoader>(_ =>
        {
            var settings = new ConnectionSettings(new Uri(elasticConfiguration.Host!))
                .DefaultIndex(elasticConfiguration.IndexName);

            return new SearchEngineLoader(new ElasticClient(settings), elasticConfiguration);
        });

        services.AddHttpClient<ICurrencyApiProvider, CurrencyApiProvider>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["CurrencyApiUrl"]);
        });

        services.AddSingleton<ICurrencyHandler, CurrencyHandler>();
        services.AddHostedService<ElasticCurrencyLoader>();
    })
    .Build();

// only for demo. This is not a production ready code
using (var scope = host.Services.CreateScope())
{
    var searchEngineLoader = scope.ServiceProvider.GetRequiredService<ISearchEngineLoader>();
    await searchEngineLoader.CreateIndexAsync();
}

await host.RunAsync();