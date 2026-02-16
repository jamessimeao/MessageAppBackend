using JWTAuth.Dtos;
using JWTAuth.Models;

namespace JWTAuth.Data
{
    public interface IDataAccess
    {
        public Task RegisterUserAsync(User user);
        public Task<bool> UserExistsAsync(UserRegisterDto userDto);
        public Task<User?> GetUserFromIdAsync(int userId);
        public Task<User?> GetUserFromEmailAsync(string userEmail);
        public Task SaveRefreshTokenAsync(int userId, RefreshTokenData refreshTokenData);
        public Task<RefreshTokenData?> GetRefreshTokenDataAsync(int userId);
    }
}
