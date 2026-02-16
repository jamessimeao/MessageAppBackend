using System.Text.Json;

namespace Message.Kafka.Keys
{
    public class Key
    {
        public required int SenderId { get; set; }
        public required int ReceiverId { get; set; }
        public required DateTime Time { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
