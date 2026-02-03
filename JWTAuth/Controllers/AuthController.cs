using JWTAuth.Dtos;
using JWTAuth.Models;
using JWTAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWTAuth.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> TestConnection()
        {
            Console.WriteLine("action TestConnection");
            return Ok("Reached Auth endpoint");
        }

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

        // The access token will expire after some time. If the refresh token didn't expire yet,
        // this action will give a JWT with a new access token and same refresh token.
        // If the refresh token is invalid, the user will have to login again.
        // The [Authorize] will be used without checking if the access token is outdated, since the purpose
        // of the endpoint is to get a new access token.
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TokenDto?>> RefreshAccessTokenAsync(string refreshToken)
        {
            // The refresh token should only be passed to this endpoint,
            // to avoid interception, so it is a parameter of the request.
            // The access token is always passed through the headers.
            // The authorization schema will get the claims for the user
            // that were obtained from the access token.
            // We search in the claims for the user id.
            string? userIdString = null;
            foreach(ClaimsIdentity identity in HttpContext.User.Identities)
            {
                Claim? claim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if(claim != null)
                {
                    userIdString = claim.Value;
                }
            }

            if (userIdString == null)
            {
                return Unauthorized();
            }

            int userId;
            bool parsed = int.TryParse(userIdString, out userId);
            if(parsed == false)
            {
                return Unauthorized();
            }

            // Request for a new token, with a new access token (but same refresh token)
            TokenDto? token = await authService.RefreshAccessTokenAsync(userId, refreshToken);
            if(token == null)
            {
                return Forbid();
            }
            return Ok(token);
        }
    }
}
