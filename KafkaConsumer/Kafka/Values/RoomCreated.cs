using System.Text.Json;

namespace KafkaConsumer.Kafka.Values
{
    public class RoomCreated
    {
        public required int RoomId { get; set; }
        public required int UserId { get; set; }
    }
}
