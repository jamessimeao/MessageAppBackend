using JWTAuth.Data;
using JWTAuth.Dtos;
using JWTAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens; // for SymmetricSecurityKey
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text; // for PasswordHasher

namespace JWTAuth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDataAccess _dataAccess;
        // For token creation
        private readonly string appSettingsToken;
        private readonly string appSettingsIssuer;
        private readonly string appSettingsAudience;
        private const double MINUTES_TO_EXPIRE_ACCESS_TOKEN = 30;
        private const double DAYS_TO_EXPIRE_REFRESH_TOKEN = 1;
        private const uint REFRESH_TOKEN_BYTES = 32;

        public AuthService(IConfiguration configuration, IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;

            // For token creation
            appSettingsToken = configuration.GetValue<string>("AppSettings:Token")
                                        ?? throw new Exception("Failed to get AppSettings token.");
            appSettingsIssuer = configuration.GetValue<string>("AppSettings:Issuer")
                                        ?? throw new Exception("Failed to get AppSettings issuer.");
            appSettingsAudience = configuration.GetValue<string>("AppSettings:Audience")
                                        ?? throw new Exception("Failed to get AppSettings audience.");
        }

        public async Task<User?> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            // First check if the user already exists in the database
            bool userExists = await _dataAccess.UserExistsAsync(userRegisterDto);
            if (userExists)
            {
                return null;
            }

            // The user doesn't exist, so create a new one
            User user = new User();
            // First create a hash of the password
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, userRegisterDto.Password);
            // Then set other properties of user
            user.Email = userRegisterDto.Email;
            user.Username = userRegisterDto.Username;

            // Add the new user to the database
            await _dataAccess.RegisterUserAsync(user);

            return user;
        }

        public async Task<TokenDto?> LoginAsync(UserLoginDto userLoginDto)
        {
            // First get the user in the database with given email.
            // If there is no user with such email, we get a null.
            User? user = await _dataAccess.GetUserFromEmailAsync(userLoginDto.Email);
            if(user == null)
            {
                return null;
            }

            // Check if the given password corresponds to the password hash from the database
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            PasswordVerificationResult passwordVerificationResult =
                passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
            if(passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return null;
            }

            // Here the login was successful, then return a token for authentication of the user
            TokenDto token = await CreateToken(user);
            return token;
        }

        private async Task<string> CreateAccessToken(User user)
        {
            Dictionary<string, object> claims = new Dictionary<string, object>()
            {
                [ClaimTypes.Email] = user.Email,
                [ClaimTypes.NameIdentifier] = user.Id.ToString(),
                [ClaimTypes.Name] = user.Username,
                [ClaimTypes.Role] = user.UserRole.ToString(),
            };

            byte[] data = Encoding.UTF8.GetBytes(appSettingsToken);
            SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(data);
            SigningCredentials signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = appSettingsIssuer,
                Audience = appSettingsAudience,
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(MINUTES_TO_EXPIRE_ACCESS_TOKEN),
                SigningCredentials = signingCredentials,
            };

            string accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            return accessToken;
        }

        private async Task<string> CreateRandomStringInBase64(uint bytes)
        {
            // Make random bytes
            byte[] randomBytes = new byte[bytes];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Create string from random bytes
            string str = Convert.ToBase64String(randomBytes);
            return str;
        }

        private async Task<string> CreateRefreshTokenAndSaveToDb(int userId)
        {
            // Create the refresh token
            string refreshToken = await CreateRandomStringInBase64(REFRESH_TOKEN_BYTES);
            DateTime refreshTokenExpirationTime = DateTime.UtcNow.AddDays(DAYS_TO_EXPIRE_REFRESH_TOKEN);
            RefreshTokenData refreshTokenData = new RefreshTokenData()
            {
                RefreshToken = refreshToken,
                RefreshTokenExpirationTime = refreshTokenExpirationTime
            };

            // Save to database
            await _dataAccess.SaveRefreshTokenAsync(userId, refreshTokenData);

            // Return the refresh token
            return refreshToken;
        }

        private async Task<TokenDto> CreateToken(User user)
        {
            Task<string> accessTokenTask = CreateAccessToken(user);
            Task<string> refreshTokenTask = CreateRefreshTokenAndSaveToDb(user.Id);
            string accessToken = await accessTokenTask;
            string refreshToken = await refreshTokenTask;

            TokenDto token = new TokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

            return token;
        }

        private async Task<bool> RefreshTokenIsValid(int userId, string refreshToken)
        {
            RefreshTokenData? refreshTokenData = await _dataAccess.GetRefreshTokenDataAsync(userId);
            if(refreshTokenData != null)
            {
                bool sameRefreshToken = refreshToken == refreshTokenData.RefreshToken;
                DateTime now = DateTime.UtcNow;
                bool notExpired = now <= refreshTokenData.RefreshTokenExpirationTime;

                if (sameRefreshToken && notExpired)
                {
                    return true;
                }
            }

            return false;
        }

        // The access token will expire after some time. This method gives a new access token if the refresh token didn't expire yet.
        // If the refresh token is invalid, the user will have to login again.
        public async Task<TokenDto?> RefreshAccessTokenAsync(int userId, string refreshToken)
        {
            // Validate refresh token
            bool refreshTokenIsValid = await RefreshTokenIsValid(userId, refreshToken);
            if (!refreshTokenIsValid)
            {
                return null;
            }

            // Get user from its id
            User? user = await _dataAccess.GetUserFromIdAsync(userId);
            if(user == null)
            {
                return null;
            }

            // The refresh token is valid, so create a new access token, but keep the same refresh token.
            // After some time the refresh token will become invalid. When that happens, the user must
            // login again, which will give a new refresh token.
            string accessToken = await CreateAccessToken(user);
            TokenDto newToken = new TokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            return newToken;
        }
    }
}
