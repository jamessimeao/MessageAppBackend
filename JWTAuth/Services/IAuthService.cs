using JWTAuth.Dtos;
using JWTAuth.Models;
using System.Globalization;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        public Task<User?> RegisterAsync(UserDto userDto);
        public Task<TokenDto?> LoginAsync(UserDto userDto);
        public Task<TokenDto?> RefreshTokenAsync(int userId, string refreshToken);
    }
}
