using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace Message.Kafka.Keys
{
    public class KeySerializer : ISerializer<Key>
    {
        public byte[] Serialize(Key data, SerializationContext context)
        {
            string serializedToString = JsonSerializer.Serialize(data);
            byte[] bytes = Encoding.UTF8.GetBytes(serializedToString);
            return bytes;
        }
    }
}
