using Rooms.Roles;

namespace Rooms.Data
{
    public interface IDataAccess
    {
        //*********************** rooms table **********************************
        public Task<int> CreateRoomAsync(string name);
        public Task DeleteRoomAsync(int roomId);
        public Task UpdateRoomNameAsync(int roomId, string name);

        //*********************** usersrooms table *****************************
        public Task AddUserToRoomAsync(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<int> CountUsersInRoomAsync(int roomId);
        public Task RemoveUserFromRoomAsync(int roomId, int userId);
        public Task UpdateUserRoleInRoom(int roomId, int userId, RoleInRoom roleInRoom);
        public Task<RoleInRoom?> GetRoleInRoomForUser(int roomId, int userId);

        //*********************** users table **********************************
        public Task<int> GetUserIdFromEmail(string userEmail);
    }
}
