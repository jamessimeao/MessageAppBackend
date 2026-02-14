using Message.Data;
using Message.Kafka.Producer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


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
        private const string roomsIdsKey = "roomsIds";

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
            bool hasValueRoomId = Context.Items.TryGetValue(roomsIdsKey, out valueRoomId);
            if (!hasValueRoomId || valueRoomId == null)
            {
                Console.WriteLine("Null rooms ids in Context.Items");
                return;
            }
            IEnumerable<int> roomsIds = (IEnumerable<int>)valueRoomId;

            // Remove from corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (int roomId in roomsIds)
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
                Console.WriteLine("Null user id in Context.Items");
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
            Console.WriteLine("Adding user id to Context.Items");
            Context.Items.Add(userIdKey, userId);
            return true;
        }

        private string GroupName(int roomId)
        {
            return Convert.ToString(roomId);
        }

        private bool UserIsInRoom(int roomId)
        {
            IEnumerable<int> roomsIds;
            object? value;
            bool hasValue = Context.Items.TryGetValue(roomsIdsKey, out value);
            if (!hasValue || value == null)
            {
                Console.WriteLine("Null rooms ids in Context.Items");
                return false;
            }

            roomsIds = (IEnumerable<int>)value;
            if (!roomsIds.Contains(roomId))
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
                Console.WriteLine("Null user id in Context.Items");
                return;
            }
            int userId = (int)valueUserId;

            // Get from the database which groups the user is in.
            Console.WriteLine("ChatHub adding user to its groups...");
            IEnumerable<int> roomsIds = await _dataAccess.GetRoomsIdsAsync(userId);
            // Since the Hub is transient, I can't store the rooms in the class,
            // but I can store it in Context.Items.
            Context.Items.Add(roomsIdsKey, roomsIds);

            // Add to corresponding groups in SignalR.
            List<Task> tasks = new List<Task>();
            foreach (int roomId in roomsIds)
            {
                string groupName = GroupName(roomId);
                tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, groupName));
            }
            await Task.WhenAll(tasks);
        }
    }
}
