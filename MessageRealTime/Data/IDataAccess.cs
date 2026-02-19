namespace MessageRealTime.Data
{
    public interface IDataAccess
    {
        //public Task<int> GetUserIdAsync(string userEmail);
        public Task<IEnumerable<int>> GetUsersIdsFromRoom(int roomId);
        public Task<int> SaveMessageAsync(int roomId, int senderId, string content, DateTime time);
    }
}
