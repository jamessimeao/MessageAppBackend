using MessageREST.Data;
using MessageREST.Dtos;
using MessageREST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessageREST.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class MessageController(IDataAccess dataAccess) : ControllerBase
    {
        private int maxMessagesQuantity = 50;

        // From Rooms controller
        private async Task<int?> GetUserIdFromEmail(ClaimsPrincipal user)
        {
            string? userEmail = user.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return null;
            }
            int userId = await dataAccess.GetUserIdFromEmail(userEmail);
            return userId;
        }

    }
}
