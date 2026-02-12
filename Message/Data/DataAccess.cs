namespace Message.Data
{
    public class DataAccess : IDataAccess
    {
        public async Task<IEnumerable<int>> GetRoomIdsAsync(int userId)
        {
            IEnumerable<int> roomIds = [0];
            return roomIds;
        }
    }
}
