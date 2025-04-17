using CleaningMyName.Api.Models.Requests;
using CleaningMyName.Api.Models.Responses;
using CleaningMyName.Application.Authentication.Commands.Login;
using CleaningMyName.Application.Authentication.Commands.RefreshToken;
using Microsoft.AspNetCore.Mvc;

namespace CleaningMyName.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>Authentication response with token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await Mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(ApiResponse.ErrorResponse(result.Error));
        }

        var authData = result.Value;
        var response = new AuthResponse
        {
            UserId = authData.UserId,
            UserName = authData.UserName,
            Token = authData.Token,
            RefreshToken = authData.RefreshToken,
            Roles = authData.Roles,
            Expiration = DateTime.UtcNow.AddHours(1) // Based on your JWT expiration
        };

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Authentication successful"));
    }

    /// <summary>
    /// Refreshes an expired JWT token
    /// </summary>
    /// <param name="request">The refresh token</param>
    /// <returns>New authentication response with fresh tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await Mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(ApiResponse.ErrorResponse(result.Error));
        }

        var authData = result.Value;
        var response = new AuthResponse
        {
            UserId = authData.UserId,
            UserName = authData.UserName,
            Token = authData.Token,
            RefreshToken = authData.RefreshToken,
            Roles = authData.Roles,
            Expiration = DateTime.UtcNow.AddHours(1) // Based on your JWT expiration
        };

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully"));
    }
}
