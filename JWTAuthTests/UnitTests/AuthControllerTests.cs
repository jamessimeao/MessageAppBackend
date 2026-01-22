using JWTAuth.Controllers;
using JWTAuth.Dtos;
using JWTAuth.Models;
using JWTAuth.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JWTAuthTests.UnitTests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task RegisterAsync_ReturnsCreated_ForNewUser()
        {
            //************************* Arrange *********************
            // The new user to be registered
            UserRegisterDto userRegisterDto = new UserRegisterDto()
            {
                Email = "name@company.com",
                Password = "password",
                Username = "name",
            };

            // Not the actual user that the IAuthService will return, but a non null user
            User user = new User();

            // A mock for the IAuthService, that returns a non null user for the newly registered user
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(authService => authService.RegisterAsync(userRegisterDto)).Returns(Task.FromResult<User?>(user));

            // Create the authController
            AuthController authController = new AuthController(authServiceMock.Object);

            //************************* Act *********************
            ActionResult result = await authController.RegisterAsync(userRegisterDto);
            IStatusCodeActionResult statusCodeActionResult = (IStatusCodeActionResult)result;
            int? statusCode = statusCodeActionResult.StatusCode;

            //************************* Assert *********************
            Assert.NotNull(result);
            Assert.NotNull(statusCode);
            Assert.Equal(201, statusCode); // 201 = created
        }

        [Fact]
        public async Task RegisterAsync_ReturnsBadRequest_ForUserThatAlreadyExists()
        {
            //************************* Arrange *********************
            // The user to be registered.
            UserRegisterDto userRegisterDto = new UserRegisterDto()
            {
                Email = "name@company.com",
                Password = "password",
                Username = "name",
            };

            // A mock for the IAuthService.
            // The user should be already registered,
            // so the IAuthService must return null.
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(authService => authService.RegisterAsync(userRegisterDto)).Returns(Task.FromResult<User?>(null));

            // Create the authController
            AuthController authController = new AuthController(authServiceMock.Object);

            //************************* Act *********************
            ActionResult result = await authController.RegisterAsync(userRegisterDto);
            IStatusCodeActionResult statusCodeActionResult = (IStatusCodeActionResult)result;
            int? statusCode = statusCodeActionResult.StatusCode;

            //************************* Assert *********************
            Assert.NotNull(result);
            Assert.NotNull(statusCode);
            Assert.Equal(400, statusCode); // 400 = bad request
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_ForRegisteredUser()
        {
            //************************* Arrange *********************
            // The user to login.
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "name@company.com",
                Password = "password",
            };

            // Token to be returned from IAuthService.LoginAsync
            TokenDto token = new TokenDto()
            {
                AccessToken = "accessToken",
                RefreshToken = "refreshToken",
            };

            // A mock for the IAuthService.
            // The user should be registered,
            // so the IAuthService must return a non null token.
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(authService => authService.LoginAsync(userLoginDto)).Returns(Task.FromResult<TokenDto?>(token));

            // Create the authController
            AuthController authController = new AuthController(authServiceMock.Object);

            //************************* Act *********************
            ActionResult<TokenDto?> result = await authController.LoginAsync(userLoginDto);

            IStatusCodeActionResult? statusCodeActionResult = (IStatusCodeActionResult?) result.Result;
            int? statusCode = statusCodeActionResult?.StatusCode;

            // The value isn't stored in result.Value, rather in result.Result.
            // To extract it, we need to do some casts.
            TokenDto? tokenResponse = null;
            if(result.Result != null)
            {
                tokenResponse = (TokenDto?)((ObjectResult)result.Result).Value;
            }

            //************************* Assert *********************
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.NotNull(statusCode);
            Assert.NotNull(tokenResponse);
            Assert.Equal(200, statusCode); // 200 = ok
        }

        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_ForNonRegisteredUser()
        {
            //************************* Arrange *********************
            // The user to login.
            UserLoginDto userLoginDto = new UserLoginDto()
            {
                Email = "name@company.com",
                Password = "password",
            };

            // A mock for the IAuthService.
            // The user shouldn't be registered,
            // so the IAuthService must return a null token.
            Mock<IAuthService> authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(authService => authService.LoginAsync(userLoginDto)).Returns(Task.FromResult<TokenDto?>(null));

            // Create the authController
            AuthController authController = new AuthController(authServiceMock.Object);

            //************************* Act *********************
            ActionResult<TokenDto?> result = await authController.LoginAsync(userLoginDto);

            IStatusCodeActionResult? statusCodeActionResult = (IStatusCodeActionResult?)result.Result;
            int? statusCode = statusCodeActionResult?.StatusCode;

            //************************* Assert *********************
            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            Assert.NotNull(statusCode);
            Assert.Equal(400, statusCode); // 400 = bad request
        }
    }
}
