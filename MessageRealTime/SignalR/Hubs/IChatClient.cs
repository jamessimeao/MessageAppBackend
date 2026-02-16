namespace Message.SignalR.Hubs
{
    // Interface that abstracts the Hub from the client.
    // It is used by SignalR for strongly typing.
    // Instead of passing a string with the name of a client's method,
    // we use a method from this interface.
    public interface IChatClient
    {
        public Task ReceiveMessageAsync(int senderId, string message, DateTime time);
        public Task ReceiveErrorMessageAsync(string errorMessage);
    }
}
