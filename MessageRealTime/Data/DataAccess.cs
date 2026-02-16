using Dapper;
using System.Data;

namespace Message.Data
{
    public class DataAccess(IDbConnection connection) : IDataAccess
    {
        private const string USERID_VARIABLE = "userid";
        private const string EMAIL_VARIABLE = "email";

        private const string GET_USER_ROOMS_PROCEDURE = "dbo.getUserRooms";
        private const string GET_USER_ID_FROM_EMAIL_PROCEDURE = "dbo.getUserIdFromEmail";

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
        }

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
    }
}
