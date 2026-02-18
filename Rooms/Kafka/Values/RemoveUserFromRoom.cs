using System.Text.Json;

namespace Rooms.Kafka.Values
{
    public class RemoveUserFromRoom
    {
        public required int RoomId { get; set; }
        public required int UserId { get; set; }
    }
}
