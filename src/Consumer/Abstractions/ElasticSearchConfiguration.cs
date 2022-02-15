using Microsoft.Extensions.Options;

namespace Consumer.Abstractions
{
    public class ElasticSearchConfiguration : IOptions<ElasticSearchConfiguration>
    {
        public string? Host { get; set; }
        public string? IndexName { get; set; }

        public ElasticSearchConfiguration Value => this;
    }
}
