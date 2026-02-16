using JWTAuth.Roles;

namespace JWTAuth.Dtos
{
    public class UserRegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Username { get; set; }
    }
}
