using JWTAuth.Dtos;
using JWTAuth.Models;

namespace JWTAuth.Services
{
    public class AuthService : IAuthService
    {
        public async Task<User?> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            throw new NotImplementedException();
        }

        public async Task<TokenDto?> LoginAsync(UserLoginDto userLoginDto)
        {
            throw new NotImplementedException();
        }

        // The access token will expire after some time. This method gives a new access token if the refresh token didn't expire yet.
        // If the refresh token is invalid, the user will have to login again.
        public async Task<TokenDto?> RefreshTokenAsync(int userId, string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
