namespace Message.Data
{
    public interface IDataAccess
    {
        public Task<IEnumerable<int>> GetRoomsIdsAsync(int userId);

        public Task<int> GetUserIdAsync(string userEmail);
    }
}
