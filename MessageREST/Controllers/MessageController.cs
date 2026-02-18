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
    public class MessageController(IDataAccess dataAccess, IKafkaProducer kafkaProducer) : ControllerBase
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

        //************************************ Actions **********************************************
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> LoadLatestMessages(LoadLatestMessagesDto loadLatestMessagesDto)
        {
            // Check if user is in the room
            int? userId = await GetUserIdFromEmail(User);
            if(userId == null)
            {
                return Unauthorized();
            }

            bool userIsInRoom = await dataAccess.UserIsInRoom(loadLatestMessagesDto.RoomId, userId.Value);
            if (!userIsInRoom)
            {
                return Forbid();
            }

            if (loadLatestMessagesDto.Quantity > maxMessagesQuantity)
            {
                return BadRequest($"Can't request for more than {maxMessagesQuantity} messages.");
            }

            IEnumerable<Message> messages = await dataAccess.LoadLatestMessagesAsync
                                            (
                                                loadLatestMessagesDto.RoomId,
                                                loadLatestMessagesDto.Quantity
                                            );
            return Ok(messages);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> LoadMessagesPrecedingReference(
            LoadMessagesPrecedingReferenceDto loadMessagesPrecedingRefDto)
        {
            // Check if user is in the room
            int? userId = await GetUserIdFromEmail(User);
            if(userId == null)
            {
                return Unauthorized();
            }

            bool userIsInRoom = await dataAccess.UserIsInRoom(loadMessagesPrecedingRefDto.RoomId, userId.Value);
            if (!userIsInRoom)
            {
                return Forbid();
            }

            if (loadMessagesPrecedingRefDto.Quantity > maxMessagesQuantity)
            {
                return BadRequest($"Can't request for more than {maxMessagesQuantity} messages.");
            }

            IEnumerable<Message> messages = await dataAccess.LoadMessagesPrecedingReferenceAsync
                                            (
                                                loadMessagesPrecedingRefDto.RoomId,
                                                loadMessagesPrecedingRefDto.MessageIdReference,
                                                loadMessagesPrecedingRefDto.Quantity
                                            );

            return Ok(messages);
        }

        [HttpPut]
        public async Task<ActionResult> EditMessage(EditMessageDto editMessageDto)
        {
            // Check if user owns the message
            int? userId = await GetUserIdFromEmail(User);
            if (User == null)
            {
                return Unauthorized();
            }

            bool userOwnsMessage = await dataAccess.UserOwnsMessage(userId.Value, editMessageDto.MessageId);
            if(!userOwnsMessage)
            {
                return Forbid();
            }

            await dataAccess.EditMessageAsync(editMessageDto.MessageId, editMessageDto.NewContent);

            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteMessage(DeleteMessageDto deleteMessageDto)
        {
            // Check if user owns the message
            int? userId = await GetUserIdFromEmail(User);
            if (User == null)
            {
                return Unauthorized();
            }

            bool userOwnsMessage = await dataAccess.UserOwnsMessage(userId.Value, deleteMessageDto.MessageId);
            if (!userOwnsMessage)
            {
                return Forbid();
            }

            await dataAccess.DeleteMessageAsync(deleteMessageDto.MessageId);

            return Ok();
        }
    }
}
