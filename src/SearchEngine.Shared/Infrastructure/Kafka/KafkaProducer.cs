using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using SearchEngine.Shared.Abstractions;
using System.Diagnostics;
using System.Text;

namespace SearchEngine.Shared.Infrastructure.Kafka
{
    public class KafkaProducer<T> : IKafkaProducer<T>, IDisposable where T : class
    {
        private static readonly ActivitySource ActivitySource = new("Producer");
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        private readonly IProducer<Guid, T> _producer;
        private readonly KafkaSettings _kafkaSettings;

        private bool _disposed;

        public KafkaProducer(IOptions<KafkaSettings> kafkaOptions)
        {
            ArgumentNullException.ThrowIfNull(kafkaOptions, nameof(kafkaOptions));

            _kafkaSettings = kafkaOptions.Value!;

            var producerConfiguration = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.Server
            };

            _producer = new ProducerBuilder<Guid, T>(producerConfiguration)
                .SetKeySerializer(GuidKafkaSerializer.Instance)
                .SetValueSerializer(JsonKafkaSerializer<T>.Instance)
                .Build();
        }

        public Task ProduceAsync(T record)
        {
            using var activity = ActivitySource.StartActivity("message send", ActivityKind.Producer);
            ActivityContext contextToInject = activity?.Context ?? Activity.Current?.Context ?? default;

            var message = new Message<Guid, T>
            {
                Key = Guid.NewGuid(),
                Value = record
            };

            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), message, InjectTraceContext);

            return _producer.ProduceAsync(_kafkaSettings.Topic, message);

            void InjectTraceContext(Message<Guid, T> messageProperties, string key, string value)
            {
                messageProperties.Headers ??= new Headers();
                messageProperties.Headers.Add(key, Encoding.UTF8.GetBytes(value));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (!_disposed)
            {
                try
                {
                    _producer?.Dispose();
                }
                catch (Exception)
                {
                }
            }

            _disposed = true;
        }
    }
}
