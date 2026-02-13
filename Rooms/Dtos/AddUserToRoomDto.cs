using Rooms.Roles;

namespace Rooms.Dtos
{
    public class AddUserToRoomDto
    {
        public required int RoomId { get; set; }
        public required string UserEmail { get; set; }
        public required RoleInRoom RoleInRoom { get; set; }
    }
}
