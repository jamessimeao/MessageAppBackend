using Dapper;
using REST.Dtos.Rooms;
using REST.Models;
using REST.Roles;
using System.Data;

namespace REST.Data
{
    public class DataAccess(IDbConnection connection) : IDataAccess
    {
        private const string ROOMID_VARIABLE = "roomid";
        private const string MESSAGEIDREFERENCE_VARIABLE = "messageidreference";
        private const string QUANTITY_VARIABLE = "quantity";
        private const string MESSAGEID_VARIABLE = "messageid";
        private const string NEWCONTENT_VARIABLE = "newcontent";
        private const string USERID_VARIABLE = "userid";

        private const string LOAD_LATEST_MESSAGES_PROCEDURE = "dbo.loadLatestMessages";
        private const string LOAD_MESSAGES_PROCEDURE = "dbo.loadMessagesPrecedingReference";
        private const string EDIT_MESSAGE_PROCEDURE = "dbo.editMessage";
        private const string DELETE_MESSAGE_PROCEDURE = "dbo.deleteMessage";
        private const string DELETE_USER_MESSAGES_FROM_ROOM_PROCEDURE = "dbo.deleteUserMessagesFromRoom";
        private const string GET_MESSAGE_OWNER_PROCEDURE = "dbo.getMessageOwner";
        private const string USER_IS_IN_ROOM_PROCEDURE = "dbo.userIsInRoom";

        // From Rooms project
        // users table
        //private const string EMAIL_VARIABLE = "email";

        //private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

        private const string ROOM_HAS_USER_WITH_ROLE_PROCEDURE = "dbo.roomHasUserWithRole";
        private const string SET_USERS_ROLE_IN_ROOM_PROCEDURE = "dbo.setUsersRoleInRoom";

        public async Task<IEnumerable<Message>> LoadLatestMessagesAsync(int roomId, uint quantity)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(QUANTITY_VARIABLE, quantity);

            IEnumerable<Message> messages = await connection.QueryAsync<Message>
            (
                LOAD_LATEST_MESSAGES_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return messages;
        }

        public async Task<IEnumerable<Message>> LoadMessagesPrecedingReferenceAsync(int roomId, int messageIdReference, uint quantity)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(MESSAGEIDREFERENCE_VARIABLE, messageIdReference);
            parameters.Add(QUANTITY_VARIABLE, quantity);

            IEnumerable<Message> messages = await connection.QueryAsync<Message>
            (
                LOAD_MESSAGES_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return messages;
        }

        public async Task EditMessageAsync(int messageId, string newContent)
        {
            DynamicParameters parameters = new();
            parameters.Add(MESSAGEID_VARIABLE, messageId);
            parameters.Add(NEWCONTENT_VARIABLE, newContent);

            await connection.ExecuteAsync
            (
                EDIT_MESSAGE_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            DynamicParameters parameters = new();
            parameters.Add(MESSAGEID_VARIABLE, messageId);

            await connection.ExecuteAsync
            (
                DELETE_MESSAGE_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteUserMessagesFromRoomAsync(int roomId, int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);

            await connection.ExecuteAsync
            (
                DELETE_USER_MESSAGES_FROM_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> UserOwnsMessageAsync(int userId, int messageId)
        {
            DynamicParameters parameters = new();
            parameters.Add(MESSAGEID_VARIABLE, messageId);

            int ownerId = await connection.QuerySingleAsync<int>
            (
                GET_MESSAGE_OWNER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userId == ownerId;
        }

        public async Task<bool> UserIsInRoomAsync(int roomId, int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE , userId);

            bool userIsInRoom = await connection.QuerySingleAsync<bool>
            (
                USER_IS_IN_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userIsInRoom;
        }

        public async Task<bool> UserIsARoomAdminAsync(int roomId, int userId)
        {
            // Get the role the user has for the room with given id
            RoleInRoom? roleInRoom = await GetRoleInRoomForUserAsync(roomId, userId);

            if (roleInRoom == null || roleInRoom != RoleInRoom.Admin)
            {
                // user is not in room
                return false;
            }

            return true;
        }

        public async Task<bool> RoomHasUserWithRoleAsync(int roomId, RoleInRoom roleInRoom)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);

            bool hasAdmin = await connection.QuerySingleAsync<bool>
                            (
                                ROOM_HAS_USER_WITH_ROLE_PROCEDURE,
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
            return hasAdmin;
        }

        public async Task SetUsersRoleInRoomAsync(int roomId, RoleInRoom roleInRoom)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);

            await connection.ExecuteAsync
            (
                SET_USERS_ROLE_IN_ROOM_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        // rooms table
        private const string ID_VARIABLE = "id";
        private const string NAME_VARIABLE = "name";

        private const string CREATE_ROOM_PROCEDURE = "dbo.createRoom";
        private const string DELETE_ROOM_PROCEDURE = "dbo.deleteRoom";
        private const string UPDATE_ROOM_NAME_PROCEDURE = "dbo.updateRoomName";

        // usersrooms table
        //private const string ROOMID_VARIABLE = "roomid";
        //private const string USERID_VARIABLE = "userid";
        private const string ROLEINROOM_VARIABLE = "roleinroom";

        private const string ADD_USER_TO_ROOM_PROCEDURE = "dbo.addUserToRoom";
        private const string COUNT_USERS_IN_ROOM_PROCEDURE = "dbo.countUsersInRoom";
        private const string REMOVE_USER_FROM_ROOM = "dbo.removeUserFromRoom";
        private const string UPDATE_USER_ROLE_IN_ROOM_PROCEDURE = "dbo.updateUserRoleInRoom";
        private const string GET_ROLE_IN_ROOM_FOR_USER_PROCEDURE = "dbo.getRoleInRoomForUser";
        private const string GET_ROOM_NAME_PROCEDURE = "dbo.getRoomName";
        private const string GET_USERS_INFO_FROM_ROOM_PROCEDURE = "dbo.getUsersInfoFromRoom";

        // users table
        //private const string EMAIL_VARIABLE = "email";

        //private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

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

        public async Task<RoomInfoDto> GetRoomInfoAsync(int roomId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);

            RoomInfoDto roomInfo = await connection.QuerySingleAsync<RoomInfoDto>
                                (
                                    GET_ROOM_NAME_PROCEDURE,
                                    parameters,
                                    commandType: CommandType.StoredProcedure
                                );
            return roomInfo;
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

        public async Task UpdateUserRoleInRoomAsync(int roomId, int userId, RoleInRoom roleInRoom)
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

        public async Task<RoleInRoom?> GetRoleInRoomForUserAsync(int roomId, int userId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(USERID_VARIABLE, userId);

            // Enums store integers, but, for safety,
            // the enums are being in the database as strings.
            // First get the string, then parse it to the enum.
            string? roleString = await connection.QuerySingleOrDefaultAsync<string>
            (
                GET_ROLE_IN_ROOM_FOR_USER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            if (roleString == null)
            {
                return null;
            }

            RoleInRoom roleInRoom = Enum.Parse<RoleInRoom>(roleString);

            return roleInRoom;
        }

        //************************************* users table *********************************************
        /*
        public async Task<int> GetUserIdFromEmailAsync(string userEmail)
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
        }*/

        //********************************* mixed *************************************
        public async Task<IEnumerable<UserInfoDto>> GetUsersInfoFromRoomAsync(int roomId)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);

            IEnumerable<UserInfoDto> usersInfo = await connection.QueryAsync<UserInfoDto>
                                                    (
                                                        GET_USERS_INFO_FROM_ROOM_PROCEDURE,
                                                        parameters,
                                                        commandType: CommandType.StoredProcedure
                                                    );
            return usersInfo;
        }
    }
}
