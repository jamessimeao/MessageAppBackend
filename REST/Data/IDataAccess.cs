using REST.Models;
using REST.Roles;

namespace REST.Data
{
    public interface IDataAccess
    {
        //************************** messages table ****************************
        public Task<IEnumerable<Message>> LoadLatestMessagesAsync(int roomId, uint quantity);
        public Task<IEnumerable<Message>> LoadMessagesPrecedingReferenceAsync(int roomId, int messageIdReference, uint quantity);
        public Task EditMessageAsync(int messageId, string newContent);
        public Task DeleteMessageAsync(int messageId);
        public Task<bool> UserOwnsMessage(int userId, int messageId);

        //*********************** rooms table **********************************
        public Task<int> CreateRoomAsync(string name);
        public Task DeleteRoomAsync(int roomId);
        public Task UpdateRoomNameAsync(int roomId, string name);
        public Task<string> GetRoomNameAsync(int roomId);

        //*********************** usersrooms table *****************************
        public Task AddUserToRoomAsync(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<int> CountUsersInRoomAsync(int roomId);
        public Task RemoveUserFromRoomAsync(int roomId, int userId);
        public Task UpdateUserRoleInRoom(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<RoleInRoom?> GetRoleInRoomForUser(int roomId, int userId);
        public Task<bool> UserIsInRoom(int roomId, int userId);
        public Task<bool> UserIsARoomAdmin(int roomId, int userId);
        public Task<bool> RoomHasUserWithRole(int roomId, RoleInRoom roleInRoom);
        public Task SetUsersRoleInRoom(int roomId, RoleInRoom roleInRoom);

        //*********************** users table **********************************
        //public Task<int> GetUserIdFromEmail(string userEmail);
    }
}
