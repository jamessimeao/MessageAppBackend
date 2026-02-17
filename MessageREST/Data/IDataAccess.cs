using MessageREST.Models;

namespace MessageREST.Data
{
    public interface IDataAccess
    {
        public Task<IEnumerable<Message>> LoadMessagesAsync(int roomId, int messageIdReference, uint quantity);
        public Task EditMessageAsync(int messageId, string newMessage);
        public Task DeleteMessageAsync(int messageId);
        public Task<bool> UserOwnsMessage(int userId, int messageId);
        public Task<bool> UserIsInRoom(int roomId, int userId);

        //*********************** users table **********************************
        public Task<int> GetUserIdFromEmail(string userEmail); // copied from Rooms project
    }
}
