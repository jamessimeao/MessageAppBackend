using MessageRealTime.Data;
using MessageRealTime.Kafka.Producer;
using MessageRealTime.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


/*
    The Hub is transient, it can be disposed fast. For this reason, it can't be used for long tasks.
    The Hub.Context is null in the constructor, as far as my tests show, only use it in the methods.
*/

namespace MessageRealTime.SignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IDataAccess _dataAccess;
        //private readonly IKafkaProducer _kafkaProducer;

        // Keys to be used with Context.Items dictionary
        private const string userIdKey = "userId";
        private const string roomsIdsKey = "roomsIds";

        public ChatHub(IConfiguration configuration, IDataAccess dataAccess)//, IKafkaProducer kafkaProducer)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Constructing ChatHub...");

            _dataAccess = dataAccess;
            //_kafkaProducer = kafkaProducer;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            bool succesful = await GetUserId();
            if(!succesful)
            {
                throw new Exception("Error: Failed to get user id.");
            }
            await AddToGroupsAsync();

            NotificationDto notificationDto = new()
            {
                Content = "Connected." 
            };
            await Clients.Caller.ReceiveNotificationAsync(notificationDto);
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
                Console.WriteLine("Error: Null rooms ids in Context.Items");
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

        public async Task SendMessageAsync(SendMessageDto sendMessageDto)
        {
            if (!UserIsInRoom(sendMessageDto.RoomId))
            {
                Console.WriteLine("Error: User trying to send message to room it is not in.");
                return;
            }

            object? valueUserId;
            bool hasValueUserId = Context.Items.TryGetValue(userIdKey, out valueUserId);
            if (!hasValueUserId || valueUserId == null)
            {
                Console.WriteLine("Error: Null user id in Context.Items");
                return;
            }
            int senderId = (int)valueUserId;

            // First save the message in the database, from which the message gets an id.
            // This id will be sent to the client as part of the message.
            Console.WriteLine("ChatHub saving message to database...");
            int messageId = await _dataAccess.SaveMessageAsync
                            (
                                sendMessageDto.RoomId,
                                senderId,
                                sendMessageDto.Content,
                                sendMessageDto.Time
                            );
            
            Console.WriteLine("ChatHub sending message to group...");
            string groupName = GroupName(sendMessageDto.RoomId);
            ReceiveMessageDto receiveMessageDto = new()
            {
                Id = messageId,
                RoomId = sendMessageDto.RoomId,
                SenderId = senderId,
                Content = sendMessageDto.Content,
                Time = sendMessageDto.Time,
            };
            await Clients.Group(groupName).ReceiveMessageAsync(receiveMessageDto);
            
            /*Console.WriteLine("ChatHub sending message to Kafka...");
            Task kafkaTask = _kafkaProducer.ProduceToKafkaAsync
                            (
                                messageId,
                                senderId,
                                sendMessageDto.RoomId,
                                sendMessageDto.Content,
                                sendMessageDto.Time
                            );

            await messageTask;
            await kafkaTask;*/
        }

        /*
        public async Task<bool> UpdateUserRooms()
        {
            // Get the user id from Context.Items
            object? valueUserId;
            bool hasValueUserId = Context.Items.TryGetValue(userIdKey, out valueUserId);
            if (!hasValueUserId || valueUserId == null)
            {
                Console.WriteLine("Error: Null user id in Context.Items");
                return false;
            }
            int userId = (int)valueUserId;

            // Get the updated list of rooms ids
            IOrderedEnumerable<int> roomsIdsNew = (await _dataAccess.GetRoomsIdsAsync(userId)).Order();

            // Get old list of rooms ids
            IOrderedEnumerable<int> roomsIdsOld;
            object? value;
            bool hasValue = Context.Items.TryGetValue(roomsIdsKey, out value);
            if (!hasValue || value == null)
            {
                Console.WriteLine("Error: Null rooms ids in Context.Items");
                return false;
            }
            roomsIdsOld = ((IEnumerable<int>)value).Order();

            // Update the SignalR groups the user is in.
            // Each SignalR group corresponds to a room.
            IEnumerator<int> enumeratorOld = roomsIdsOld.GetEnumerator();
            IEnumerator<int> enumeratorNew = roomsIdsNew.GetEnumerator();
            
            bool notTraversedOld = true;
            bool notTraversedNew = true;
            int roomIdOld;
            int roomIdNew;
            string groupName;
            List<Task> tasks = new();
            while (notTraversedOld && notTraversedNew)
            {
                roomIdOld = enumeratorOld.Current;
                roomIdNew = enumeratorNew.Current;
                if (roomIdOld < roomIdNew)
                {
                    // Remove from old room.
                    groupName = GroupName(roomIdOld);
                    tasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName));
                    notTraversedOld = enumeratorOld.MoveNext();
                }
                else if (roomIdOld >= roomIdNew)
                {
                    // Add to new room.
                    // If roomIdOld == roomIdNew, this shouldn't be necessary,
                    // but add again for safety. It is safe to again to the group again.
                    groupName = GroupName(roomIdNew);
                    tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, groupName));
                    notTraversedNew = enumeratorNew.MoveNext();
                }
            }
            // Remove from all old rooms that remained
            while(notTraversedOld)
            {
                // Remove from old room.
                roomIdOld = enumeratorOld.Current;
                groupName = GroupName(roomIdOld);
                tasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName));
                notTraversedOld = enumeratorOld.MoveNext();
            }
            // Add to all new rooms that remained
            while(notTraversedNew)
            {
                // Add to new room.
                roomIdNew = enumeratorNew.Current;
                groupName = GroupName(roomIdNew);
                tasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, groupName));
                notTraversedNew = enumeratorNew.MoveNext();
            }

            await Task.WhenAll(tasks);

            return true;
        }
        */

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
                Console.WriteLine("Error: Null rooms ids in Context.Items");
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
                Console.WriteLine("Error: Null user id in Context.Items");
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
