using Confluent.Kafka;
using Message.Kafka.Keys;

namespace Message.Kafka.Producer
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly string bootstrapServers;
        private const string topic = "my-topic";

        // Kafka producer
        private readonly IProducer<Key, string> producer; // Create a producer for each connection
        private readonly TimeSpan flushTimeOut = TimeSpan.FromSeconds(1);

        public KafkaProducer(IConfiguration configuration)
        {
            Console.WriteLine("Constructing KafkaProducer...");

            // Get bootstrapServers, for Kafka
            {
                string? kbs = configuration.GetValue<string>("kafkaBootstrapServers");
                if (kbs == null)
                {
                    throw new Exception("Couldn't get kafkaBootstrapServers from configuration files.");
                }
                bootstrapServers = kbs;
            }

            // Kafka producer
            ProducerConfig producerConfig = new ProducerConfig()
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
            };

            producer = new ProducerBuilder<Key, string>(producerConfig)
                        .SetKeySerializer(new KeySerializer())
                        .Build();
        }

        public async Task ProduceToKafkaAsync(int senderId, int receiverId, string message, DateTime time)
        {
            // There will be multiple hubs, one for each container that runs the application.
            // As such, a user connected to some container can't send a message to a user in another container
            // by useing the same hub. Instead, the message will be sent to Kafka.
            Console.WriteLine("KafkaProducer sending message to Kafka...");

            Message<Key, string> kafkaMessage = new()
            {
                Key = new Key()
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Time = time,
                },
                Value = message,
            };

            producer.Produce
            (
                topic,
                kafkaMessage,
                (DeliveryReport<Key, string> deliveryReport) =>
                {
                    if(deliveryReport.Error != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to send message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced event to topic {topic}: key = {kafkaMessage.Key}, value = {kafkaMessage.Value}");
                    }
                }
            );

            producer.Flush(flushTimeOut);
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            GC.SuppressFinalize(this);
            producer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
