using JWTAuth.Roles;
using System.ComponentModel.DataAnnotations;

namespace JWTAuth.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = String.Empty;
        public string PasswordHash { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public UserRoles UserRole { get; set; } = UserRoles.Regular;
    }
}
