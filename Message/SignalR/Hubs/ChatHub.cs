using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Message.Kafka.Producer;
using Message.Data;


/*
    The Hub is transient, it can be disposed fast. For this reason, it can't be used for long tasks.
    The Hub.Context is null in the constructor, as far as my tests show, only use it in the methods.
*/

namespace Message.SignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IDataAccess _dataAccess;
        private readonly IKafkaProducer _kafkaProducer;

        // Keys to be used with Context.Items dictionary
        private const string userIdKey = "userId";
        private const string roomIdsKey = "roomIds";

        public ChatHub(IConfiguration configuration, IDataAccess dataAccess, IKafkaProducer kafkaProducer)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Constructing ChatHub...");

            _dataAccess = dataAccess;
            _kafkaProducer = kafkaProducer;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            bool succesful = await GetUserId();
            if(!succesful)
            {
                throw new Exception("Failed to get user id.");
            }
            await AddToGroupsAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            Console.WriteLine("User disconnected.");
            await base.OnDisconnectedAsync(exception);
            // Check if some device for the same user is still connected.
            // If not, remove the user from its SignalR groups.
        }

        public async Task RemoveFromGroupsAsync()
        {
            // Get the room ids
            object? valueRoomId;
            bool hasValueRoomId = Context.Items.TryGetValue(roomIdsKey, out valueRoomId);
            if (!hasValueRoomId || valueRoomId == null)
            {
                return;
            }
            IEnumerable<int> roomIds = (IEnumerable<int>)valueRoomId;

            // Remove from corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (int roomId in roomIds)
            {
                string groupName = GroupName(roomId);
                tasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName));
            }
            await Task.WhenAll(tasks);
        }

        public async Task SendMessageAsync(int roomId, string message, DateTime time)
        {
            if (!UserIsInRoom(roomId))
            {
                Console.WriteLine("User trying to send message to room it is not in.");
                return;
            }

            object? valueUserId;
            bool hasValueUserId = Context.Items.TryGetValue(userIdKey, out valueUserId);
            if (!hasValueUserId || valueUserId == null)
            {
                return;
            }
            int senderId = (int)valueUserId;

            Console.WriteLine("ChatHub sending message to Kafka...");
            Task kafkaTask = _kafkaProducer.ProduceToKafkaAsync(senderId, roomId, message, time);
            Console.WriteLine("ChatHub sending message to group...");
            string groupName = GroupName(roomId);
            Task messageTask = Clients.Group(groupName).ReceiveMessageAsync(senderId, message, time);

            await kafkaTask;
            await messageTask;
        }

        private async Task<bool> GetUserId()
        {
            string? userEmail = Context.UserIdentifier;
            if (userEmail == null)
            {
                Console.WriteLine("Error: Context.UserIdentifier = null in SendMessageAsync");
                return false;
            }

            int userId = await _dataAccess.GetUserIdAsync(userEmail);
            Context.Items.Add(userIdKey, userId);
            return true;
        }

        private string GroupName(int roomId)
        {
            return Convert.ToString(roomId);
        }

        private bool UserIsInRoom(int roomId)
        {
            IEnumerable<int> roomIds;
            object? value;
            bool hasValue = Context.Items.TryGetValue(roomIdsKey, out value);
            if (!hasValue || value == null)
            {
                return false;
            }

            roomIds = (IEnumerable<int>)value;
            if (!roomIds.Contains(roomId))
            {
                return false;
            }

            return true;
        }

        private async Task AddToGroupsAsync()
        {
            object? valueUserId;
            bool hasValueUserId = Context.Items.TryGetValue(userIdKey, out valueUserId);
            if (!hasValueUserId || valueUserId == null)
            {
                return;
            }
            int userId = (int)valueUserId;

            // Get from the database which groups the user is in.
            Console.WriteLine("ChatHub adding user to its groups...");
            IEnumerable<int> roomIds = await _dataAccess.GetRoomIdsAsync(userId);
            // Since the Hub is transient, I can't store the rooms in the class,
            // but I can store it in Context.Items.
            Context.Items.Add(roomIdsKey, roomIds);

            // Add to corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (int roomId in roomIds)
            {
                string groupName = GroupName(roomId);
                tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, groupName));
            }
            await Task.WhenAll(tasks);
        }
    }
}
