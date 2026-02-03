using Message.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Message.Kafka.Consumer
{
    public class KafkaConsumerOnBackground : BackgroundService
    {
        private readonly IKafkaConsumer _kafkaConsumer;

        public KafkaConsumerOnBackground(IKafkaConsumer kafkaConsumer)
        {
            _kafkaConsumer = kafkaConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _kafkaConsumer.ConsumeMessagesFromKafkaAsync(stoppingToken);
        }
    }
}
