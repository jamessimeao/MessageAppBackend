namespace MessageRealTime.Kafka
{
    public interface IConsumer : IAsyncDisposable
    {
        public Task ConsumeMessagesFromKafkaAsync(CancellationToken stoppingToken);
    }
}