using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Message.Kafka.Producer;
using Message.Data;

/*
    The Hub is transient, it can be disposed fast. For this reason, it can't be used for long tasks.
    The Hub.Context is null in the constructor, as far as my tests show, only use it in the methods.
*/

namespace Message.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IDataAccess _dataAccess;
        private readonly IKafkaProducer _kafkaProducer;

        public ChatHub(IConfiguration configuration, IDataAccess dataAccess, IKafkaProducer kafkaProducer)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Constructing ChatHub...");

            _dataAccess = dataAccess;
            _kafkaProducer = kafkaProducer;
        }

        private async Task<Tuple<string, bool>> GetUserId()
        {
            string? userId = Context.UserIdentifier;
            if (userId == null)
            {
                Console.WriteLine("Error: Context.UserIdentifier = null in SendMessageAsync");
                return new Tuple<string, bool>("",false);
            }
            return new Tuple<string, bool>(userId, true);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await AddToGroupsAsync();
        }
        private async Task AddToGroupsAsync()
        {
            (string userId, bool succeded) = await GetUserId();
            if (!succeded)
            {
                return;
            }

            // Get from the database which groups the user is in.
            Console.WriteLine("ChatHub adding user to its groups...");
            IEnumerable<string> roomIds = await _dataAccess.GetRoomIds(userId);
            // Since the Hub is transient, I can't store the rooms in the class,
            // but I can store it in Context.Items.
            Context.Items.Add("roomIds", roomIds);

            // Add to corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (string roomId in roomIds)
            {
                tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, roomId));
            }
            await Task.WhenAll(tasks);
        }

        public async Task RemoveFromGroupsAsync()
        {
            // Get from the database which groups the user is in.
            string? userId = Context.UserIdentifier;
            if (userId == null)
            {
                Console.WriteLine("Error: Context.UserIdentifier = null in SendMessageAsync");
                await Clients.Caller.ReceiveErrorMessageAsync("Error in setup.");
                return;
            }

            // Since the Hub is transient, I can't store the rooms in the class.
            IEnumerable<string> roomIds = await _dataAccess.GetRoomIds(userId);

            // Remove from corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (string roomId in roomIds)
            {
                tasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId));
            }
            await Task.WhenAll(tasks);
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
