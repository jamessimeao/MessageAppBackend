using REST.Kafka.Keys;

namespace REST.Kafka.Producer
{
    public interface IKafkaProducer : IAsyncDisposable
    {
        public Task ProduceToKafkaAsync(Key key, string serializedValue);
    }
}