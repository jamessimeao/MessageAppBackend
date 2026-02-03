using System.Text;
using System.Text.Json;

namespace Message.Kafka.Keys
{
    public class Key
    {
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
