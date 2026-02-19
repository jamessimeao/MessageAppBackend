using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REST.Data;
using REST.Dtos.Rooms;
using REST.Kafka.EventTypes;
using REST.Kafka.Keys;
using REST.Kafka.Producer;
using REST.Kafka.Values;
using REST.Roles;
using REST.Utils;

namespace REST.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class RoomsController(IDataAccess dataAccess, IKafkaProducer kafkaProducer) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> CreateRoomAndAddUserToItAsync(CreateRoomDto createRoomDto)
        {
            // Create a new room and get its id
            int roomId = await dataAccess.CreateRoomAsync(createRoomDto.Name);

            // Get the user id
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Add the user to the room
            RoleInRoom roleInRoom = RoleInRoom.Admin;
            await dataAccess.AddUserToRoomAsync(roomId, userId.Value, roleInRoom);

            Key key = new()
            {
                EventType = EventType.ROOM_CREATED_EVENT,
            };

            RoomCreated value = new()
            {
                RoomId = roomId,
                UserId = userId.Value,
            };

            // Produce an event
            await kafkaProducer.ProduceToKafkaAsync(key, Serializer<RoomCreated>.Serialize(value));
            
            return Ok(roomId);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteRoomAsync(DeleteRoomDto deleteRoomDto)
        {
            // Get the user id
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            // First check if the user has authority to delete the room
            bool authorized = await dataAccess.UserIsARoomAdmin(deleteRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.DeleteRoomAsync(deleteRoomDto.RoomId);

            Key key = new()
            {
                EventType = EventType.ROOM_DELETED_EVENT,
            };

            RoomDeleted value = new()
            {
                RoomId = deleteRoomDto.RoomId
            };

            // Produce an event
            await kafkaProducer.ProduceToKafkaAsync(key, Serializer<RoomDeleted>.Serialize(value));

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateRoomNameAsync(UpdateRoomNameDto updateRoomNameDto)
        {
            // First check if the user has authority to update the room
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await dataAccess.UserIsARoomAdmin(updateRoomNameDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.UpdateRoomNameAsync(updateRoomNameDto.RoomId, updateRoomNameDto.Name);

            return Ok();
        }

        //*******************************************************************

        [HttpDelete]
        public async Task<ActionResult> RemoveUserFromRoomAsync(RemoveUserFromRoomDto removeUserFromRoomDto)
        {
            // First check if the user has authority to do it.
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool isAdmin = await dataAccess.UserIsARoomAdmin(removeUserFromRoomDto.RoomId, userId.Value);

            // User can remove itself. If it is an admin of the room, it can remove anyone.
            bool canRemoveUser = isAdmin || userId == removeUserFromRoomDto.UserId;
            if (!canRemoveUser)
            {
                return Forbid();
            }

            await dataAccess.RemoveUserFromRoomAsync(removeUserFromRoomDto.RoomId, removeUserFromRoomDto.UserId);

            // If the room became empty, it should be deleted from the database.
            int usersRemaining = await dataAccess.CountUsersInRoomAsync(removeUserFromRoomDto.RoomId);
            if(usersRemaining == 0)
            {
                await dataAccess.DeleteRoomAsync(removeUserFromRoomDto.RoomId);
                return Ok();
            }

            // Also, if it is not empty, but don't have admins, all users will be turned into admins.
            // Check if there are admins.
            bool roomHasAdmins = await dataAccess.RoomHasUserWithRole(removeUserFromRoomDto.RoomId, RoleInRoom.Admin);
            if(!roomHasAdmins)
            {
                await dataAccess.SetUsersRoleInRoom(removeUserFromRoomDto.RoomId, RoleInRoom.Admin);
                return Ok();
            }

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserRoleInRoom(UpdateUserRoleInRoomDto updateUserRoleInRoomDto)
        {
            // First check if the user has authority to do it.
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await dataAccess.UserIsARoomAdmin(updateUserRoleInRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.UpdateUserRoleInRoom
            (
                updateUserRoleInRoomDto.RoomId,
                updateUserRoleInRoomDto.UserId,
                updateUserRoleInRoomDto.RoleInRoom
            );

            return Ok();
        }
    }
}
