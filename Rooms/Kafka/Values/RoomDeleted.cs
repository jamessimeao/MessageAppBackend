using System.Text.Json;

namespace Rooms.Kafka.Values
{
    public class RoomDeleted
    {
        public required int RoomId { get; set; }
    }
}
