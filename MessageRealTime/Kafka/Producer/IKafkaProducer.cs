using MessageRealTime.Kafka.Keys;

namespace MessageRealTime.Kafka.Producer
{
    public interface IKafkaProducer : IAsyncDisposable
    {
        public Task ProduceToKafkaAsync(Key key, string value);
    }
}