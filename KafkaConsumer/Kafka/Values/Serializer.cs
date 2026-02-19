using System.Text.Json;

namespace KafkaConsumer.Kafka.Values
{
    public class Serializer
    {
        private static JsonSerializerOptions options = new();

        public Serializer()
        {
            options.PropertyNameCaseInsensitive = true;
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value);
        }

        public T? Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, options);
        }
    }
}
