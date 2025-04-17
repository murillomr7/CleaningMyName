using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;

namespace CleaningMyName.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    private readonly ILogger<HealthCheckController> _logger;
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckController(
        ILogger<HealthCheckController> logger,
        HealthCheckService healthCheckService)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
    }

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

    /// <summary>
    /// Detailed health check that reports the status of all registered health checks
    /// </summary>
    /// <returns>Detailed health report</returns>
    [HttpGet("detailed")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetailedHealthReport()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Duration = report.TotalDuration,
            HealthChecks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration,
                Description = e.Value.Description
            })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    /// <summary>
    /// Custom health check response that formats the result as JSON
    /// </summary>
    [HttpGet("custom")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCustomHealthCheck()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var json = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        });

        return Content(json, "application/json");
    }
}
