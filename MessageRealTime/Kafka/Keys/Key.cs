using System.Text.Json;
using MessageRealTime.Kafka.EventTypes;

namespace MessageRealTime.Kafka.Keys
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
