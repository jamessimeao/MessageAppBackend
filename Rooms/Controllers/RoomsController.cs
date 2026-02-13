using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rooms.Data;
using Rooms.Roles;
using System.Security.Claims;

namespace Rooms.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class RoomsController(IDataAccess dataAccess) : ControllerBase
    {
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

        private async Task<bool> IsUserAuthorizedToConfigureRoom(int roomId, int userId)
        {
            // Get the role the user has for the room with given id
            RoleInRoom? roleInRoom = await dataAccess.GetRoleInRoomForUser(roomId, userId);
            if (roleInRoom == null || roleInRoom != RoleInRoom.Admin)
            {
                // user is not in room
                return false;
            }

            return true;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateRoomAndAddUserToItAsync(string name)
        {
            // Create a new room and get its id
            int roomId = await dataAccess.CreateRoomAsync(name);

            // Get the user id
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Add the user to the room
            await dataAccess.AddUserToRoomAsync(roomId, userId.Value, RoleInRoom.Admin);
            
            return Ok(roomId);
        }

    }
}
