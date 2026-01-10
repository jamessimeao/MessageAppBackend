using JWTAuth.Dtos;
using JWTAuth.Models;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        public Task<User?> RegisterAsync(UserDto userDto);
        public Task<TokenDto?> LoginAsync(UserDto userDto);
        public Task<TokenDto?> RefreshTokenAsync(TokenDto oldTokenDto);
    }
}
