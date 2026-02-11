using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Message.Kafka.Producer;

/*
    The Hub is transient, it can be disposed fast. For this reason, it can't be used for long tasks.
    The Hub.Context is null in the constructor, as far as my tests show, only use it in the methods.
*/

namespace Message.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IKafkaProducer _kafkaProducer;

        public ChatHub(IConfiguration configuration, IKafkaProducer kafkaProducer)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Constructing ChatHub...");

            _kafkaProducer = kafkaProducer;
        }

        public async Task SendMessageAsync(string roomId, string message)
        {
            string? senderId = Context.UserIdentifier;
            if (senderId == null)
            {
                Console.WriteLine("Error: Context.UserIdentifier = null in SendMessageAsync");

                await Clients.Caller.ReceiveErrorMessageAsync("Failed to deliver message.");

                return;
            }

            Console.WriteLine("ChatHub sending message to Kafka...");
            Task kafkaTask = _kafkaProducer.ProduceToKafkaAsync(senderId, roomId, message);
            Console.WriteLine("ChatHub sending message to group...");
            Task messageTask = Clients.Group(roomId).ReceiveMessageAsync(senderId, message);

            await kafkaTask;
            await messageTask;
        }
    }
}
