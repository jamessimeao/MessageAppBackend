using System.Text.Json;

namespace MessageRealTime.Kafka.Values
{
    public static class Serializer<T>
    {
        public static string Serialize(T value)
        {
            return JsonSerializer.Serialize(value);
        }

        public static T? Deserialize(string value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}
