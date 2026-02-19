using MessageRealTime.Data;
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

        // Keys to be used with Context.Items dictionary
        private const string userIdKey = "userId";

        public ChatHub(IConfiguration configuration, IDataAccess dataAccess)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Constructing ChatHub...");

            _dataAccess = dataAccess;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            bool succesful = await GetUserId();
            if(!succesful)
            {
                throw new Exception("Error: Failed to get user id.");
            }

            NotificationDto notificationDto = new()
            {
                Content = "Connected.",
            };
            await Clients.Caller.ReceiveNotificationAsync(notificationDto);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            Console.WriteLine("User disconnected.");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageAsync(SendMessageDto sendMessageDto)
        {
            // Get the user id from Context.Items
            object? valueUserId;
            bool hasValueUserId = Context.Items.TryGetValue(userIdKey, out valueUserId);
            if (!hasValueUserId || valueUserId == null)
            {
                Console.WriteLine("Error: Null user id in Context.Items");
                return;
            }
            int senderId = (int)valueUserId;

            // Get all users from the room
            IEnumerable<int> usersIds = await _dataAccess.GetUsersIdsFromRoom(sendMessageDto.RoomId);
            if(!usersIds.Contains(senderId))
            {
                Console.WriteLine("User can't send a message to a room it is not in.");
                return;
            }

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
            
            Console.WriteLine("ChatHub sending message to room...");
            ReceiveMessageDto receiveMessageDto = new()
            {
                Id = messageId,
                RoomId = sendMessageDto.RoomId,
                SenderId = senderId,
                Content = sendMessageDto.Content,
                Time = sendMessageDto.Time,
            };

            // Get the users from the room without the sender
            IEnumerable<int> usersIdsExceptSender = usersIds.Except([senderId]);

            // Convert int to string
            IEnumerable<string> usersIdsToSend = usersIdsExceptSender.Select(id => id.ToString());

            // Send message to all users in room except itself
            await Clients.Users(usersIdsToSend).ReceiveMessageAsync(receiveMessageDto);
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
    }
}
