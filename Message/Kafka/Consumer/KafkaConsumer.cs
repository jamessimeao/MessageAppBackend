using Confluent.Kafka;
using Message.Hubs;
using Message.Kafka.Keys;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace Message.Kafka.Consumer
{
    public class KafkaConsumer : IKafkaConsumer
    {
        // Kafka consumer
        private readonly string bootstrapServers;
        private const string topic = "my-topic";
        private const string groupId = "someGroupId";
        private readonly IConsumer<string, string> consumer;

        private readonly IHubContext<ChatHub, IChatClient> _hubContext;

        public KafkaConsumer(IConfiguration configuration, IHubContext<ChatHub, IChatClient> hubContext)
        {
            Console.WriteLine("Constructing KafkaConsumer...");

            _hubContext = hubContext;

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
                        await SendMessageBackToSignalRClient(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nError sending message back: {ex.Message}\n");
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

        private async Task SendMessageBackToSignalRClient(ConsumeResult<string, string> consumeResult)
        {
            Console.WriteLine("KafkaConsumer sending message back to client...");

            string serializedKey = consumeResult.Message.Key;
            Key? key = JsonSerializer.Deserialize<Key>(serializedKey);
            if (key == null)
            {
                throw new Exception("Error: Null key.");
            }
            string message = consumeResult.Message.Value;
            string groupName = key.ReceiverId;
            await _hubContext.Clients.Group(groupName).ReceiveMessageAsync(key.SenderId, message);

            Console.WriteLine("KafkaConsumer sent back to client.");
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            GC.SuppressFinalize(this);
            consumer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
