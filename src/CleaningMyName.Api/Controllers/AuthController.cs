using CleaningMyName.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CleaningMyName.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // For testing purposes only - in production, validate credentials
            if (request.Username == "test" && request.Password == "test")
            {
                var token = _jwtService.GenerateToken(
                    "1", 
                    request.Username, 
                    new List<string> { "User" }
                );

                return Ok(new { token });
            }

            return Unauthorized();
        }

        [HttpGet("secure")]
        [Authorize]
        public IActionResult Secure()
        {
            return Ok(new { message = "This is a secure endpoint!" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
