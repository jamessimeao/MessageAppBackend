using Dapper;
using System.Data;

namespace MessageRealTime.Data
{
    public class DataAccess(IDbConnection connection) : IDataAccess
    {
        private const string USERID_VARIABLE = "userid";
        private const string EMAIL_VARIABLE = "email";

        //private const string GET_USER_ROOMS_PROCEDURE = "dbo.getUserRooms";
        private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

        // Messages table
        private const string ROOMID_VARIABLE = "roomid";
        private const string SENDERID_VARIABLE = "senderid";
        private const string CONTENT_VARIABLE = "content";
        private const string TIME_VARIABLE = "time";

        private const string SAVE_MESSAGE_PROCEDURE = "dbo.saveMessage";

        /*
        public async Task<IEnumerable<int>> GetRoomsIdsAsync(int userId)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(USERID_VARIABLE, userId);

            IEnumerable<int> roomIds = await connection.QueryAsync<int>
            (
                GET_USER_ROOMS_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return roomIds;
        }*/

        public async Task<int> GetUserIdAsync(string userEmail)
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

        public async Task<int> SaveMessageAsync(int roomId, int senderId, string content, DateTime time)
        {
            DynamicParameters parameters = new();
            parameters.Add(ROOMID_VARIABLE, roomId);
            parameters.Add(SENDERID_VARIABLE, senderId);
            parameters.Add(CONTENT_VARIABLE, content);
            parameters.Add(TIME_VARIABLE, time);

            int messageId = await connection.QuerySingleAsync<int>
                            (
                                SAVE_MESSAGE_PROCEDURE,
                                parameters,
                                commandType: CommandType.StoredProcedure
                            );
            return messageId;
        }
    }
}
