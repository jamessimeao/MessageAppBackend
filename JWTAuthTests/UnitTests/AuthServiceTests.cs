using JWTAuth.Data;
using JWTAuth.Dtos;
using JWTAuth.Models;
using JWTAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JWTAuthTests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly IConfigurationRoot _configuration;

        public AuthServiceTests()
        {
            // Configuration
            Dictionary<string, string?> configurationDict = new()
            {
                { "AppSettings:Token", "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789"},
                { "AppSettings:Issuer", "issuer"},
                { "AppSettings:Audience", "audience"},
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            _configuration = configurationBuilder.AddInMemoryCollection(configurationDict).Build();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUser_ForNewUser()
        {
            //********************* Arrange *************************
            UserRegisterDto userRegisterDto = new()
            {
                Email = "name@company.com",
                Password = "password",
                Username = "name",
            };

            PasswordHasher<User> passwordHasher = new();

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.UserExistsAsync(userRegisterDto)).Returns(Task.FromResult(false));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            User? user = await authService.RegisterAsync(userRegisterDto);
            PasswordVerificationResult? result = null;
            if(user != null)
            {
                result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userRegisterDto.Password);
            }

            //********************* Assert *************************
            Assert.NotNull(user);
            Assert.NotNull(result);
            Assert.Equal(userRegisterDto.Email, user.Email);
            Assert.Equal(userRegisterDto.Username, user.Username);
            Assert.Equal(PasswordVerificationResult.Success, result);
        }
    }
}
