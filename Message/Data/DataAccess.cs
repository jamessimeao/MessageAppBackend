using Dapper;
using System.Data;

namespace Message.Data
{
    public class DataAccess(IDbConnection connection) : IDataAccess
    {
        private const string USERID_VARIABLE = "userid";

        private const string GET_USER_ROOMS_PROCEDURE = "dbo.getUserRooms";

        public async Task<IEnumerable<int>> GetRoomIdsAsync(int userId)
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
            return 0;
        }
    }
}
