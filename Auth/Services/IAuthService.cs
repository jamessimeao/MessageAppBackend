using JWTAuth.Dtos;
using JWTAuth.Models;
using System.Globalization;

namespace JWTAuth.Services
{
    public interface IAuthService
    {
        public Task<User?> RegisterAsync(UserRegisterDto userRegisterDto);
        public Task<TokenDto?> LoginAsync(UserLoginDto userLoginDto);
        public Task<TokenDto?> RefreshAccessTokenAsync(int userId, string refreshToken);
        public Task DeleteAsync(int userId);
    }
}
