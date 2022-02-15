using Microsoft.Extensions.Logging;

namespace SearchEngine.Shared
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Information,
            Message = "Subscribing to topic {topic}")]
        public static partial void SubscribeToTopic(this ILogger logger, string topic);

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "Consumed a record")]
        public static partial void ConsumedARecord(this ILogger logger);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Information,
            Message = "Shutting down the service")]
        public static partial void ShuttingDown(this ILogger logger);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Error,
            Message = "Error while consuming a record.")]
        public static partial void ExceptionHandling(this ILogger logger, Exception exception);
    }
}
