using System.Text.Json;

namespace KafkaConsumer.Kafka.Values
{
    public class RoomDeleted
    {
        public required int RoomId { get; set; }
    }
}
