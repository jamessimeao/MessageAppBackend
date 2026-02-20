using REST.Roles;

namespace REST.Dtos.Rooms
{
    public class UserInfoDto
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required string RoleInRoom { get; set; }
    }
}
