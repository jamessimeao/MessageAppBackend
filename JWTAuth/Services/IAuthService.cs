using JWTAuth.Dtos;
using JWTAuth.Models;
using System.Globalization;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        public Task<TokenDto?> RefreshTokenAsync(int userId, string refreshToken);
        public Task<User?> RegisterAsync(UserRegisterDto userRegisterDto);
        public Task<TokenDto?> LoginAsync(UserLoginDto userLoginDto);
    }
}
