using Dapper;
using Rooms.Roles;
using System.Data;

namespace Rooms.Data
{
    public class DataAccess(IDbConnection connection) : IDataAccess
    {
        // rooms table
        private const string ID_VARIABLE = "id";
        private const string NAME_VARIABLE = "name";

        private const string CREATE_ROOM_PROCEDURE = "dbo.createRoom";
        private const string DELETE_ROOM_PROCEDURE = "dbo.deleteRoom";
        private const string UPDATE_ROOM_NAME_PROCEDURE = "dbo.updateRoomName";

        // usersrooms table
        private const string ROOMID_VARIABLE = "roomid";
        private const string USERID_VARIABLE = "userid";
        private const string ROLEINROOM_VARIABLE = "roleinroom";

        private const string ADD_USER_TO_ROOM_PROCEDURE = "dbo.addUserToRoom";
        private const string COUNT_USERS_IN_ROOM_PROCEDURE = "dbo.countUsersInRoom";
        private const string REMOVE_USER_FROM_ROOM = "dbo.removeUserFromRoom";
        private const string UPDATE_USER_ROLE_IN_ROOM_PROCEDURE = "dbo.updateUserRoleInRoom";
        private const string GET_ROLE_IN_ROOM_FOR_USER_PROCEDURE = "dbo.getRoleInRoomForUser";

        // users table
        private const string EMAIL_VARIABLE = "email";

        private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

        //********************************************** rooms table *****************************************
        public async Task<int> CreateRoomAsync(string name)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(NAME_VARIABLE, name);

            int roomId = await connection.QuerySingleAsync<int>
            (
                CREATE_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return roomId;
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ID_VARIABLE, roomId);

            await connection.ExecuteAsync
            (
                DELETE_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateRoomNameAsync(int roomId, string name)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ID_VARIABLE, roomId);
            parameters.Add(NAME_VARIABLE, name);

            await connection.ExecuteAsync
            (
                UPDATE_ROOM_NAME_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        //********************************************** usersrooms table *****************************************

        public async Task AddUserToRoomAsync(int roomId, int userId, RoleInRoom roleInRoom)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);
            parameters.Add(ROLEINROOM_VARIABLE, roleInRoom.ToString());

            await connection.ExecuteAsync
            (
                ADD_USER_TO_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> CountUsersInRoomAsync(int roomId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ROOMID_VARIABLE, roomId);

            int count = await connection.QuerySingleAsync<int>
            (
                COUNT_USERS_IN_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return count;
        }

        public async Task RemoveUserFromRoomAsync(int roomId, int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);

            await connection.ExecuteAsync
            (
                REMOVE_USER_FROM_ROOM,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateUserRoleInRoom(int roomId, int userId, RoleInRoom roleInRoom)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);
            parameters.Add(ROLEINROOM_VARIABLE, roleInRoom.ToString());

            await connection.ExecuteAsync
            (
                UPDATE_USER_ROLE_IN_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RoleInRoom?> GetRoleInRoomForUser(int roomId, int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);

            RoleInRoom? roleInRoom = await connection.QuerySingleOrDefaultAsync<RoleInRoom>
            (
                GET_ROLE_IN_ROOM_FOR_USER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return roleInRoom;
        }

        //************************************* users table *********************************************
        public async Task<int> GetUserIdFromEmail(string userEmail)
        {
            DynamicParameters parameters = new();
            parameters.Add(EMAIL_VARIABLE, userEmail);

            int userId = await connection.QuerySingleAsync<int>
            (
                GET_USER_ID_FROM_EMAIL_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userId;
        }
    }
}
