using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleaningMyName.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    /// <summary>
    /// Simple health check endpoint that doesn't require authentication
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
