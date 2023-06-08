using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SearchEngine.Shared.Abstractions;

namespace SearchEngine.Shared.Infrastructure.Kafka
{
    public class KafkaConsumer<T> : IKafkaConsumer<T>, IDisposable where T : class
    {
        private readonly ILogger _logger;
        private readonly IConsumer<Guid, T> _consumer;

        private readonly KafkaSettings _kafkaSettings;
        private bool _disposed;

        public KafkaConsumer(ILogger<KafkaConsumer<T>> logger, IOptions<KafkaSettings> kafkaOptions)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(kafkaOptions, nameof(kafkaOptions));

            _logger = logger;
            _kafkaSettings = kafkaOptions.Value!;

            var consumerConfig = new ConsumerConfig
            {
                GroupId = "g1",
                BootstrapServers = _kafkaSettings.Server,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<Guid, T>(consumerConfig)
                .SetKeyDeserializer(GuidKafkaSerializer.Instance)
                .SetValueDeserializer(JsonKafkaSerializer<T>.Instance)
                .Build();
        }

        public Task Subscribe(Func<ConsumeResult<Guid, T>, Task> callback, CancellationToken cancellationToken)
        {
            _logger.SubscribeToTopic(_kafkaSettings.Topic!);

            _consumer.Subscribe(_kafkaSettings.Topic);

            var taskCompletionSource = new TaskCompletionSource<bool>();

            new Thread(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var record = _consumer.Consume(cancellationToken);

                        await callback(record);

                        _consumer.Commit();
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.ShuttingDown();
                    }
                    catch (Exception exception)
                    {
                        _logger.ExceptionHandling(exception);
                    }
                }

                taskCompletionSource.SetResult(true);
            })
            {
                IsBackground = true
            }.Start();

            return taskCompletionSource.Task;
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
                    _consumer?.Close();
                    _consumer?.Dispose();
                }
                catch (Exception)
                {
                }
            }

            _disposed = true;
        }
    }
}
