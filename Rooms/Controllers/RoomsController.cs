using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rooms.Data;
using Rooms.Dtos;
using Rooms.Kafka.Keys;
using Rooms.Kafka.Producer;
using Rooms.Roles;
using System.Security.Claims;
using System.Text.Json;

namespace Rooms.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class RoomsController(IDataAccess dataAccess, IKafkaProducer kafkaProducer) : ControllerBase
    {
        private const string ROOM_CREATED_EVENT = "room-created";
        private const string ROOM_DELETED_EVENT = "room-deleted";
        private const string ADD_USER_TO_ROOM_EVENT = "add-user-to-room";

        //*****************************************************************************
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

        //********************************** Actions ***************************************************************

        [HttpPost]
        public async Task<ActionResult<int>> CreateRoomAndAddUserToItAsync(CreateRoomDto createRoomDto)
        {
            // Create a new room and get its id
            int roomId = await dataAccess.CreateRoomAsync(createRoomDto.Name);

            // Get the user id
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Add the user to the room
            RoleInRoom roleInRoom = RoleInRoom.Admin;
            await dataAccess.AddUserToRoomAsync(roomId, userId.Value, roleInRoom);

            Key key = new()
            {
                EventType = ROOM_CREATED_EVENT,
            };

            string value = JsonSerializer.Serialize(new
            {
                RoomId = roomId,
                UserId = userId.Value,
                RoleInRoom = roleInRoom,
            });

            // Produce an event
            await kafkaProducer.ProduceToKafkaAsync(key, value);
            
            return Ok(roomId);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteRoomAsync(DeleteRoomDto deleteRoomDto)
        {
            // Get the user id
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            // First check if the user has authority to delete the room
            bool authorized = await IsUserAuthorizedToConfigureRoom(deleteRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.DeleteRoomAsync(deleteRoomDto.RoomId);

            Key key = new()
            {
                EventType = ROOM_DELETED_EVENT,
            };

            string value = JsonSerializer.Serialize(new
            {
                RoomId = deleteRoomDto.RoomId,
            });

            // Produce an event
            await kafkaProducer.ProduceToKafkaAsync(key, value);

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateRoomNameAsync(UpdateRoomNameDto updateRoomNameDto)
        {
            // First check if the user has authority to update the room
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await IsUserAuthorizedToConfigureRoom(updateRoomNameDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.UpdateRoomNameAsync(updateRoomNameDto.RoomId, updateRoomNameDto.Name);
            return Ok();
        }

        //*******************************************************************
        [HttpPost]
        public async Task<ActionResult> AddUserToRoomAsync(AddUserToRoomDto addUserToRoomDto)
        {
            // First check if the user that is adding the other one to the room
            // has authority to do it.
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await IsUserAuthorizedToConfigureRoom(addUserToRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            // Get id of user to be added
            int userToAddId = await dataAccess.GetUserIdFromEmail(addUserToRoomDto.UserEmail);

            await dataAccess.AddUserToRoomAsync(addUserToRoomDto.RoomId, userToAddId, addUserToRoomDto.RoleInRoom);

            Key key = new()
            {
                EventType = ADD_USER_TO_ROOM_EVENT,
            };

            string value = JsonSerializer.Serialize(new
            {
                RoomId = addUserToRoomDto.RoomId,
                UserId = userToAddId,
            });

            // Produce an event
            await kafkaProducer.ProduceToKafkaAsync(key, value);

            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveUserFromRoomAsync(RemoveUserFromRoomDto removeUserFromRoomDto)
        {
            // First check if the user has authority to do it.
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await IsUserAuthorizedToConfigureRoom(removeUserFromRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            // Get id of user to be removed
            int userToRemoveId = await dataAccess.GetUserIdFromEmail(removeUserFromRoomDto.UserEmail);

            await dataAccess.RemoveUserFromRoomAsync(removeUserFromRoomDto.RoomId, userToRemoveId);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserRoleInRoom(UpdateUserRoleInRoomDto updateUserRoleInRoomDto)
        {
            // First check if the user has authority to do it.
            int? userId = await GetUserIdFromEmail(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await IsUserAuthorizedToConfigureRoom(updateUserRoleInRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            // Get id of user to update role
            int userToUpdateRoleId = await dataAccess.GetUserIdFromEmail(updateUserRoleInRoomDto.UserEmail);

            await dataAccess.UpdateUserRoleInRoom
            (
                updateUserRoleInRoomDto.RoomId,
                userToUpdateRoleId,
                updateUserRoleInRoomDto.RoleInRoom
            );

            return Ok();
        }
    }
}
