using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Message.UserIdProvider
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Identify the user uniquely by its email
            return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
