using System.Diagnostics.Metrics;

namespace SearchEngine.Shared.Infrastructure.Metrics
{
    public class MetricsService
    {
        private readonly Meter _meter;

        private Counter<long> RequestCounter { get; }

        public MetricsService()
        {
            _meter = new Meter("SearchEngineMetrics");
            RequestCounter = _meter.CreateCounter<long>(nameof(RequestCounter));
        }

        public void Inc() => RequestCounter.Add(1);
    }
}
