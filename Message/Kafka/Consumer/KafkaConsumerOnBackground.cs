using Message.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Message.Kafka.Consumer
{
    public class KafkaConsumerOnBackground : BackgroundService
    {
        private readonly IKafkaConsumer kafkaConsumer;

        public KafkaConsumerOnBackground(IConfiguration configuration, IHubContext<ChatHub, IChatClient> hubContext)
        {
            kafkaConsumer = new KafkaConsumer(configuration, hubContext);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await kafkaConsumer.ConsumeMessagesFromKafkaAsync(stoppingToken);
        }
    }
}
