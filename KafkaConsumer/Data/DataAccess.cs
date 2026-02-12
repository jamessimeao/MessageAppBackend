using Dapper;
using System.Data;

namespace KafkaConsumer.Data
{
    public class DataAccess(IDbConnection dbConnection) : IDataAccess
    {
        private const string SAVE_MESSAGE_PROCEDURE = "dbo.saveMessage";

        public async Task SaveMessage(int senderId, int roomId, string message, DateTime time)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("roomid", roomId);
            parameters.Add("senderid", senderId);
            parameters.Add("message", message);
            parameters.Add("time", time);

            await dbConnection.ExecuteAsync
            (
                SAVE_MESSAGE_PROCEDURE,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
