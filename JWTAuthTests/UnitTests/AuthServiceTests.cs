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

        [Fact]
        public async Task RegisterAsync_ReturnsNull_ForAlreadyRegisteredUser()
        {
            //********************* Arrange *************************
            UserRegisterDto userRegisterDto = new()
            {
                Email = "name@company.com",
                Password = "password",
                Username = "name",
            };

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.UserExistsAsync(userRegisterDto)).Returns(Task.FromResult(true));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            User? user = await authService.RegisterAsync(userRegisterDto);

            //********************* Assert *************************
            Assert.Null(user);
        }

        [Fact]
        public async Task LoginAsync_ReturnsTokenDto_IfUserExists()
        {
            //********************* Arrange *************************
            UserLoginDto userLoginDto = new()
            {
                Email = "name@company.com",
                Password = "password",
            };

            PasswordHasher<User> passwordHasher = new();
            string passwordHash = passwordHasher.HashPassword(new User(), userLoginDto.Password);

            User user = new User()
            {
                Email = userLoginDto.Email,
                PasswordHash = passwordHash,
            };

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.GetUserFromEmailAsync(userLoginDto.Email))
                .Returns(Task.FromResult<User?>(user));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            TokenDto? token = await authService.LoginAsync(userLoginDto);

            //********************* Assert *************************
            Assert.NotNull(token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_IfUserNotInDatabase()
        {
            //********************* Arrange *************************
            UserLoginDto userLoginDto = new()
            {
                Email = "name@company.com",
                Password = "password",
            };

            PasswordHasher<User> passwordHasher = new();
            string passwordHash = passwordHasher.HashPassword(new User(), userLoginDto.Password);

            User user = new User()
            {
                Email = userLoginDto.Email,
                PasswordHash = passwordHash,
            };

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.GetUserFromEmailAsync(userLoginDto.Email))
                .Returns(Task.FromResult<User?>(null));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            TokenDto? token = await authService.LoginAsync(userLoginDto);

            //********************* Assert *************************
            Assert.Null(token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_IfIncorrectPassword()
        {
            //********************* Arrange *************************
            UserLoginDto userLoginDto = new()
            {
                Email = "name@company.com",
                Password = "incorrectPassword",
            };

            PasswordHasher<User> passwordHasher = new();
            string passwordHash = passwordHasher.HashPassword(new User(), "correctPassword");

            User user = new User()
            {
                Email = userLoginDto.Email,
                PasswordHash = passwordHash,
            };

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.GetUserFromEmailAsync(userLoginDto.Email))
                .Returns(Task.FromResult<User?>(user));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            TokenDto? token = await authService.LoginAsync(userLoginDto);

            //********************* Assert *************************
            Assert.Null(token);
        }


        [Fact]
        public async Task RefreshAccessTokenAsync_ReturnsNull_IfUserDoesntExist()
        {
            //********************* Arrange *************************
            int userId = 1;
            string refreshToken = "";

            // Mock the IDataAccess
            Mock<IDataAccess> dataAccessMock = new();
            dataAccessMock.Setup(dataAccess => dataAccess.GetUserFromIdAsync(userId))
                .Returns(Task.FromResult<User?>(null));

            // Create the AuthService
            AuthService authService = new AuthService(_configuration, dataAccessMock.Object);

            //********************* Act *************************
            TokenDto? token = await authService.RefreshAccessTokenAsync(userId, refreshToken);

            //********************* Assert *************************
            Assert.Null(token);
        }
    }
}
