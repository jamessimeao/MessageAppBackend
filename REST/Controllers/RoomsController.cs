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
using System.Text.Json;

namespace REST.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class RoomsController(IDataAccess dataAccess, IKafkaProducer kafkaProducer, ISerializer serializer) : ControllerBase
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
            await kafkaProducer.ProduceToKafkaAsync(key, serializer.Serialize<RoomCreated>(value));
            
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
            bool authorized = await dataAccess.UserIsARoomAdminAsync(deleteRoomDto.RoomId, userId.Value);
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
            await kafkaProducer.ProduceToKafkaAsync(key, serializer.Serialize<RoomDeleted>(value));

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

            bool authorized = await dataAccess.UserIsARoomAdminAsync(updateRoomNameDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.UpdateRoomNameAsync(updateRoomNameDto.RoomId, updateRoomNameDto.Name);

            return Ok();
        }

        //*******************************************************************
        [HttpPost]
        public async Task<ActionResult<string>> GenerateInvitationTokenAsync(GenerateInvitationTokenDto generateInvitationTokenDto)
        {
            // First check if the user has authority to do it.
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool isAdmin = await dataAccess.UserIsARoomAdminAsync(generateInvitationTokenDto.RoomId, userId.Value);
            if (!isAdmin)
            {
                return Forbid();
            }

            // Generate an invitation to join the specified room.
            // The invitation is generic, anyone with the invitation can join the room.

            // For the moment, the invitation will be a token, which will be just the serialized generateInvitationTokenDto
            string token = JsonSerializer.Serialize(generateInvitationTokenDto);
            return Ok(token);
        }

        [HttpPost]
        public async Task<ActionResult> JoinRoomAsync(string token)
        {
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Temporary solution !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            GenerateInvitationTokenDto? decoded = JsonSerializer.Deserialize<GenerateInvitationTokenDto>(token);
            if (decoded == null)
            {
                Console.WriteLine("Failed to decode token.");
                return BadRequest();
            }
           
            // Validate token
            
            // Add user to room
            await dataAccess.AddUserToRoomAsync(decoded.RoomId, userId.Value, RoleInRoom.Regular);

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

            bool isAdmin = await dataAccess.UserIsARoomAdminAsync(removeUserFromRoomDto.RoomId, userId.Value);

            // User can remove itself. If it is an admin of the room, it can remove anyone.
            bool canRemoveUser = isAdmin || userId == removeUserFromRoomDto.UserId;
            if (!canRemoveUser)
            {
                return Forbid();
            }

            await dataAccess.RemoveUserFromRoomAsync(removeUserFromRoomDto.RoomId, removeUserFromRoomDto.UserId);

            // Also delete all messages that the user has sent to the room
            await dataAccess.DeleteUserMessagesFromRoomAsync(removeUserFromRoomDto.RoomId, removeUserFromRoomDto.UserId);

            // If the room became empty, it should be deleted from the database.
            int usersRemaining = await dataAccess.CountUsersInRoomAsync(removeUserFromRoomDto.RoomId);
            if(usersRemaining == 0)
            {
                await dataAccess.DeleteRoomAsync(removeUserFromRoomDto.RoomId);
                return Ok();
            }

            // Also, if it is not empty, but don't have admins, all users will be turned into admins.
            // Check if there are admins.
            bool roomHasAdmins = await dataAccess.RoomHasUserWithRoleAsync(removeUserFromRoomDto.RoomId, RoleInRoom.Admin);
            if(!roomHasAdmins)
            {
                await dataAccess.SetUsersRoleInRoomAsync(removeUserFromRoomDto.RoomId, RoleInRoom.Admin);
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

            bool authorized = await dataAccess.UserIsARoomAdminAsync(updateUserRoleInRoomDto.RoomId, userId.Value);
            if (!authorized)
            {
                return Forbid();
            }

            await dataAccess.UpdateUserRoleInRoomAsync
            (
                updateUserRoleInRoomDto.RoomId,
                updateUserRoleInRoomDto.UserId,
                updateUserRoleInRoomDto.RoleInRoom
            );

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<RoomInfoDto>> GetRoomInfoAsync(int roomId)
        {
            int? userId = Identification.GetUserId(User);
            if(userId == null)
            {
                return Unauthorized();
            }

            bool userIsInRoom = await dataAccess.UserIsInRoomAsync(roomId, userId.Value);
            if(!userIsInRoom)
            {
                return Forbid();
            }

            RoomInfoDto roomInfo = await dataAccess.GetRoomInfoAsync(roomId);
            return Ok(roomInfo);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetUsersInfoFromRoom(int roomId)
        {
            int? userId = Identification.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            bool userIsInRoom = await dataAccess.UserIsInRoomAsync(roomId, userId.Value);
            if (!userIsInRoom)
            {
                return Forbid();
            }

            IEnumerable<UserInfoDto> usersInfo = await dataAccess.GetUsersInfoFromRoomAsync(roomId);
            return Ok(usersInfo);
        }
    }
}
