using REST.Dtos.Rooms;
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
        public Task<bool> UserOwnsMessageAsync(int userId, int messageId);

        //*********************** rooms table **********************************
        public Task<int> CreateRoomAsync(string name);
        public Task DeleteRoomAsync(int roomId);
        public Task UpdateRoomNameAsync(int roomId, string name);
        public Task<RoomInfoDto> GetRoomInfoAsync(int roomId);

        //*********************** usersrooms table *****************************
        public Task AddUserToRoomAsync(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<int> CountUsersInRoomAsync(int roomId);
        public Task RemoveUserFromRoomAsync(int roomId, int userId);
        public Task UpdateUserRoleInRoomAsync(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<RoleInRoom?> GetRoleInRoomForUserAsync(int roomId, int userId);
        public Task<bool> UserIsInRoomAsync(int roomId, int userId);
        public Task<bool> UserIsARoomAdminAsync(int roomId, int userId);
        public Task<bool> RoomHasUserWithRoleAsync(int roomId, RoleInRoom roleInRoom);
        public Task SetUsersRoleInRoomAsync(int roomId, RoleInRoom roleInRoom);

        //*********************** users table **********************************
        //public Task<int> GetUserIdFromEmailAsync(string userEmail);
        //********************************* mixed *************************************
        public Task<IEnumerable<UserInfoDto>> GetUsersInfoFromRoomAsync(int roomId);
    }
}
