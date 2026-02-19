using Confluent.Kafka;
using REST.Kafka.Keys;

namespace REST.Kafka.Producer
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

        public async Task ProduceToKafkaAsync(Key key, string value)
        {
            Console.WriteLine("KafkaProducer sending message to Kafka...");

            Message<Key, string> kafkaMessage = new()
            {
                Key = key,
                Value = value,
            };

            producer.Produce
            (
                topic,
                kafkaMessage,
                (DeliveryReport<Key, string> deliveryReport) =>
                {
                    if(deliveryReport.Error != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Error: Failed to send message: {deliveryReport.Error.Reason}");
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
