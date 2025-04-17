using CleaningMyName.Api.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleaningMyName.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SecureResourcesController : ApiControllerBase
{
    private readonly ILogger<SecureResourcesController> _logger;

    public SecureResourcesController(ILogger<SecureResourcesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// A public endpoint that anyone can access
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublicResource()
    {
        return Ok(ApiResponse<string>.SuccessResponse("This is a public resource."));
    }

    /// <summary>
    /// An endpoint that requires authentication
    /// </summary>
    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult GetAuthenticatedResource()
    {
        return Ok(ApiResponse<string>.SuccessResponse("This is a resource for authenticated users."));
    }

    /// <summary>
    /// An endpoint that requires the Admin role
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminResource()
    {
        return Ok(ApiResponse<string>.SuccessResponse("This is a resource for administrators."));
    }

    /// <summary>
    /// An endpoint that requires a specific policy
    /// </summary>
    [HttpGet("manage-users")]
    [Authorize(Policy = "CanManageUsers")]
    public IActionResult GetUserManagementResource()
    {
        return Ok(ApiResponse<string>.SuccessResponse("This is a resource for managing users."));
    }
}
