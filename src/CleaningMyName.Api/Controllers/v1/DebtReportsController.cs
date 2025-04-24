using CleaningMyName.Api.Models.Responses;
using CleaningMyName.Application.Debts;
using CleaningMyName.Application.Debts.Processing;
using CleaningMyName.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleaningMyName.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DebtReportsController : ApiControllerBase
{
    private readonly IDebtDataService _debtDataService;
    private readonly ILogger<DebtReportsController> _logger;

    public DebtReportsController(
        IDebtDataService debtDataService,
        ILogger<DebtReportsController> logger)
    {
        _debtDataService = debtDataService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the debt summary for the current user
    /// </summary>
    /// <param name="forceRefresh">Whether to force a cache refresh</param>
    /// <returns>Debt summary for the current user</returns>
    [HttpGet("my-summary")]
    [ProducesResponseType(typeof(ApiResponse<DebtSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMySummary([FromQuery] bool forceRefresh = false)
    {
        var userId = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid user identity."));
        }

        var summary = await _debtDataService.GetUserDebtSummaryAsync(userGuid, forceRefresh);
        
        if (summary == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Debt summary not found."));
        }

        return Ok(ApiResponse<DebtSummaryDto>.SuccessResponse(summary));
    }

    /// <summary>
    /// Gets the debt summary for a specific user (Admin only)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="forceRefresh">Whether to force a cache refresh</param>
    /// <returns>Debt summary for the specified user</returns>
    [HttpGet("user/{userId}/summary")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DebtSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserSummary(Guid userId, [FromQuery] bool forceRefresh = false)
    {
        var summary = await _debtDataService.GetUserDebtSummaryAsync(userId, forceRefresh);
        
        if (summary == null)
        {
            return NotFound(ApiResponse.ErrorResponse("Debt summary not found."));
        }

        return Ok(ApiResponse<DebtSummaryDto>.SuccessResponse(summary));
    }

    /// <summary>
    /// Gets paginated debts for the current user
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated debts for the current user</returns>
    [HttpGet("my-debts")]
    [ProducesResponseType(typeof(ApiResponse<PagedDebtResult<DebtDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyDebts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid user identity."));
        }

        var pagedResult = await _debtDataService.GetPagedUserDebtsAsync(userGuid, pageNumber, pageSize);
        return Ok(ApiResponse<PagedDebtResult<DebtDto>>.SuccessResponse(pagedResult));
    }

    /// <summary>
    /// Gets the system-wide debt processing summary (Admin only)
    /// </summary>
    /// <param name="forceRefresh">Whether to force a cache refresh</param>
    /// <returns>System-wide debt processing summary</returns>
    [HttpGet("system-summary")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DebtProcessingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSystemSummary([FromQuery] bool forceRefresh = false)
    {
        var result = await _debtDataService.GetSystemDebtProcessingResultAsync(forceRefresh);
        
        if (result == null)
        {
            return NotFound(ApiResponse.ErrorResponse("System debt processing result not available."));
        }

        return Ok(ApiResponse<DebtProcessingResult>.SuccessResponse(result));
    }
}
