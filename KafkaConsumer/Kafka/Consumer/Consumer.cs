using Confluent.Kafka;
using KafkaConsumer.Data;
using KafkaConsumer.Keys;
using System.Text.Json;

namespace KafkaConsumer.Kafka
{
    public class Consumer : IConsumer
    {
        // Kafka consumer
        private readonly string bootstrapServers;
        private const string topic = "my-topic";
        private const string groupId = "someGroupId";
        private readonly IConsumer<string, string> consumer;

        private readonly IDataAccess _dataAccess;

        public Consumer(IConfiguration configuration, IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;

            Console.WriteLine("Constructing KafkaConsumer...");

            // Get bootstrapServers, for Kafka
            {
                string? kbs = configuration.GetValue<string>("kafkaBootstrapServers");
                if (kbs == null)
                {
                    throw new Exception("Couldn't get kafkaBootstrapServers from configuration files.");
                }
                bootstrapServers = kbs;
            }

            // Kafka consumer
            ConsumerConfig consumerConfig = new ConsumerConfig()
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(topic);
        }

        public async Task ConsumeMessagesFromKafkaAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("Starting to consume messages from Kafka...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string> consumeResult = consumer.Consume();
                    Console.WriteLine($"Consumed message: key = {consumeResult.Message.Key}, value = {consumeResult.Message.Value}");
                    
                    try
                    {
                        await ProcessConsumedMessage(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nError processing consumed message: {ex.Message}\n");
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task ProcessConsumedMessage(ConsumeResult<string, string> consumeResult)
        {
            Console.WriteLine("KafkaConsumer processing consumed message...");

            string serializedKey = consumeResult.Message.Key;
            Key? key = JsonSerializer.Deserialize<Key>(serializedKey);
            if (key == null)
            {
                throw new Exception("Error: Null key.");
            }
            int senderId = key.SenderId;
            int roomId = key.ReceiverId;
            DateTime time = key.Time;
            string message = consumeResult.Message.Value;

            // Store message in database
            await _dataAccess.SaveMessage(senderId, roomId, message, time);

            Console.WriteLine("KafkaConsumer consumed message successfully.");
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            GC.SuppressFinalize(this);
            consumer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
