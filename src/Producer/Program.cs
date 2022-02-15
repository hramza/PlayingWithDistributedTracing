using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using SearchEngine.Shared.Abstractions;
using SearchEngine.Shared.Infrastructure.Kafka;
using SearchEngine.Shared.Infrastructure.Metrics;
using OpenTelemetry.Instrumentation.AspNetCore;
using System.Diagnostics;

const string SourceName = "Producer";
const string MeterName = "SearchEngineMetrics";

ActivitySource activitySource = new(SourceName);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MetricsService>();

builder.Services.Configure<KafkaSettings>(options => builder.Configuration.GetSection(nameof(KafkaSettings)).Bind(options));
builder.Services.AddSingleton(typeof(IKafkaProducer<>), typeof(KafkaProducer<>));

builder.Services.AddOpenTelemetryTracing(traceBuilder =>
{
    traceBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SourceName).AddTelemetrySdk())
        .AddSource(SourceName)
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = req => !req.Request.Path.ToUriComponent().Contains("swagger", StringComparison.OrdinalIgnoreCase);
        })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
         {
             options.Endpoint = new Uri(builder.Configuration.GetValue<string>("AppSettings:OtelEndpoint"));
         });
});

builder.Services.AddOpenTelemetryMetrics(meterBuilder =>
{
    meterBuilder
        .AddHttpClientInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SourceName).AddTelemetrySdk())
        .AddMeter(MeterName)
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(builder.Configuration.GetValue<string>("AppSettings:OtelEndpoint"));
        });
});

builder.Services.Configure<AspNetCoreInstrumentationOptions>(options =>
{
    options.RecordException = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapPost("/index", async (CurrencyRequest currencyRequest, IKafkaProducer<CurrencyRequest> producer, MetricsService metricsService) =>
{
    if (currencyRequest is null) return Results.BadRequest();

    using var activity = activitySource.StartActivity(SourceName, ActivityKind.Internal)!;

    await producer.ProduceAsync(currencyRequest);

    activity?.SetTag("otel.status_code", "OK");
    activity?.SetTag("otel.status_description", "Indexed successfully");

    metricsService.Inc();

    return Results.Accepted();
});

app.Run();
