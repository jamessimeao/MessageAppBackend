using Dapper;
using MessageREST.Models;
using System.Data;

namespace MessageREST.Data
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
        private const string GET_MESSAGE_OWNER_PROCEDURE = "dbo.getMessageOwner";
        private const string USER_IS_IN_ROOM_PROCEDURE = "dbo.userIsInRoom";

        // From Rooms project
        // users table
        private const string EMAIL_VARIABLE = "email";

        private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

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

        public async Task<bool> UserOwnsMessage(int userId, int messageId)
        {
            DynamicParameters parameters = new();
            parameters.Add(MESSAGEID_VARIABLE, messageId);

            int ownerId = await connection.QuerySingleAsync
            (
                GET_MESSAGE_OWNER_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userId == ownerId;
        }

        public async Task<bool> UserIsInRoom(int roomId, int userId)
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

        // Copied from Rooms project
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
