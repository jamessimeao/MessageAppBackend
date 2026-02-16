namespace Message.Kafka.Producer
{
    public interface IKafkaProducer : IAsyncDisposable
    {
        public Task ProduceToKafkaAsync(int senderId, int receiverId, string message, DateTime time);
    }
}