using Rooms.Kafka.EventTypes;
using System.Text.Json;

namespace Rooms.Kafka.Keys
{
    public class Key
    {
        public required EventType EventType { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
