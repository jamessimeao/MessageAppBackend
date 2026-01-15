using JWTAuth.Dtos;
using JWTAuth.Models;
using JWTAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        // Create a user in the database
        [HttpPost]
        public async Task<ActionResult> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            User? user = await authService.RegisterAsync(userRegisterDto);
            if(user == null)
            {
                return BadRequest("User already exists.");
            }

            return Created();
        }

        // Login with credentials. It returns a JWT that should be used by the client to identity itself.
        [HttpPost]
        public async Task<ActionResult<TokenDto?>> LoginAsync(UserLoginDto userLoginDto)
        {
            TokenDto? token = await authService.LoginAsync(userLoginDto);
            if(token == null)
            {
                return BadRequest();
            }
            return Ok(token);
        }
    }
}
