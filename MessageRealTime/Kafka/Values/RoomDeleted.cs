using System.Text.Json;

namespace MessageRealTime.Kafka.Values
{
    public class RoomDeleted
    {
        public required int RoomId { get; set; }
    }
}
