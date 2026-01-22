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
            authServiceMock.Setup(x => x.RegisterAsync(userRegisterDto)).Returns(Task.FromResult(user));

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
    }
}
