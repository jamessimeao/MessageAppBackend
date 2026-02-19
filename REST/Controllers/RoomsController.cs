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
        [HttpPost]
        public async Task<ActionResult> AddUserToRoomAsync(AddUserToRoomDto addUserToRoomDto)
        {
            // First check if the user that is adding the other one to the room
            // has authority to do it.
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await dataAccess.UserIsARoomAdmin(addUserToRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            // Get id of user to be added
            int userToAddId = await dataAccess.GetUserIdFromEmail(addUserToRoomDto.UserEmail);

            await dataAccess.AddUserToRoomAsync(addUserToRoomDto.RoomId, userToAddId, addUserToRoomDto.RoleInRoom);

            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveUserFromRoomAsync(RemoveUserFromRoomDto removeUserFromRoomDto)
        {
            // First check if the user has authority to do it.
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool authorized = await dataAccess.UserIsARoomAdmin(removeUserFromRoomDto.RoomId, userId.Value);
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
