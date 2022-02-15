using Confluent.Kafka;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace Consumer.Workers
{
    public class ElasticCurrencyLoader : BackgroundService
    {
        private static readonly ActivitySource ActivitySource = new("Consumer");
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IKafkaConsumer<CurrencyRequest> _consumer;
        private readonly ICurrencyHandler _handler;

        public ElasticCurrencyLoader(IKafkaConsumer<CurrencyRequest> consumer, ICurrencyHandler handler) =>
            (_consumer, _handler) = (consumer, handler);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => _consumer.Subscribe(record => ProcessAsync(record, stoppingToken), stoppingToken);

        private async Task ProcessAsync(ConsumeResult<Guid, CurrencyRequest> record, CancellationToken cancellationToken)
        {
            if (!(record?.Message is { } message)) return;

            // Get propagation context from kafka record headers
            var parentContext = Propagator.Extract(default, message, ExtractTraceContext);

            // Inject header data into current context
            Baggage.Current = parentContext.Baggage;
            using var activity = ActivitySource.StartActivity(
                "message received",
                ActivityKind.Consumer,
                parentContext.ActivityContext,
                tags: new[] { new KeyValuePair<string, object?>("server", Environment.MachineName) });

            var request = message.Value;

            // Add activity tags
            AddTags(activity, request);

            // call currency API and index data response into elasticsearch
            await _handler.HandleAsync(request, cancellationToken);
        }

        static IReadOnlyCollection<string> ExtractTraceContext(Message<Guid, CurrencyRequest> record, string key)
        {
            try
            {
                if (record.Headers.TryGetLastBytes(key, out var buffer) && buffer is { Length: > 0 })
                {
                    return new[]
                    {
                        Encoding.UTF8.GetString(buffer)
                    };
                }
            }
            catch
            {
            }

            return Array.Empty<string>();
        }

        static void AddTags(Activity? activity, CurrencyRequest request)
        {
            activity?.SetTag("messaging.broker", "kafka");
            activity?.SetTag("messaging.kind", "topic");
            activity?.SetTag("messaging.data", $"From {request.From} to {request.To}");
        }
    }
}
