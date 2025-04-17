using CleaningMyName.Api.Models.Requests.Debts;
using CleaningMyName.Api.Models.Responses;
using CleaningMyName.Application.Debts;
using CleaningMyName.Application.Debts.Commands.CreateDebt;
using CleaningMyName.Application.Debts.Commands.DeleteDebt;
using CleaningMyName.Application.Debts.Commands.MarkDebtAsPaid;
using CleaningMyName.Application.Debts.Commands.UpdateDebt;
using CleaningMyName.Application.Debts.Queries.GetAllDebts;
using CleaningMyName.Application.Debts.Queries.GetDebtById;
using CleaningMyName.Application.Debts.Queries.GetDebtsByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleaningMyName.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DebtsController : ApiControllerBase
{
    private readonly ILogger<DebtsController> _logger;

    public DebtsController(ILogger<DebtsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets all debts (Admin only)
    /// </summary>
    /// <returns>List of all debts</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<DebtDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var debts = await Mediator.Send(new GetAllDebtsQuery());
        return Ok(ApiResponse<List<DebtDto>>.SuccessResponse(debts.ToList()));
    }

    /// <summary>
    /// Gets debts for the current user
    /// </summary>
    /// <returns>List of user's debts</returns>
    [HttpGet("my-debts")]
    [ProducesResponseType(typeof(ApiResponse<List<DebtDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyDebts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid user identity."));
        }

        var debts = await Mediator.Send(new GetDebtsByUserIdQuery(userGuid));
        return Ok(ApiResponse<List<DebtDto>>.SuccessResponse(debts.ToList()));
    }

    /// <summary>
    /// Gets debts for a specific user (Admin only)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user's debts</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<DebtDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var debts = await Mediator.Send(new GetDebtsByUserIdQuery(userId));
        return Ok(ApiResponse<List<DebtDto>>.SuccessResponse(debts.ToList()));
    }

    /// <summary>
    /// Gets a debt by ID
    /// </summary>
    /// <param name="id">Debt ID</param>
    /// <returns>The debt</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DebtDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var debt = await Mediator.Send(new GetDebtByIdQuery(id));
        return Ok(ApiResponse<DebtDto>.SuccessResponse(debt));
    }

    /// <summary>
    /// Creates a new debt (Admin only)
    /// </summary>
    /// <param name="request">Debt data</param>
    /// <returns>ID of the created debt</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateDebtRequest request)
    {
        var command = new CreateDebtCommand
        {
            UserId = request.UserId,
            Description = request.Description,
            Amount = request.Amount,
            DueDate = request.DueDate,
            IsPaid = request.IsPaid
        };

        var result = await Mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Error));
        }

        return Created($"/api/v1/debts/{result.Value}", ApiResponse<Guid>.SuccessResponse(result.Value, "Debt created successfully"));
    }

    /// <summary>
    /// Updates an existing debt (Admin only)
    /// </summary>
    /// <param name="id">Debt ID</param>
    /// <param name="request">Updated debt data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDebtRequest request)
    {
        var command = new UpdateDebtCommand
        {
            Id = id,
            Description = request.Description,
            Amount = request.Amount,
            DueDate = request.DueDate
        };

        var result = await Mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse.SuccessResponse("Debt updated successfully"));
    }

    /// <summary>
    /// Deletes a debt (Admin only)
    /// </summary>
    /// <param name="id">Debt ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteDebtCommand(id));

        if (result.IsFailure)
        {
            return NotFound(ApiResponse.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse.SuccessResponse("Debt deleted successfully"));
    }

    /// <summary>
    /// Marks a debt as paid or unpaid (Admin only)
    /// </summary>
    /// <param name="id">Debt ID</param>
    /// <param name="request">Paid status</param>
    /// <returns>Success response</returns>
    [HttpPatch("{id}/mark-paid")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] MarkDebtAsPaidRequest request)
    {
        var result = await Mediator.Send(new MarkDebtAsPaidCommand(id, request.IsPaid));

        if (result.IsFailure)
        {
            return NotFound(ApiResponse.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse.SuccessResponse($"Debt marked as {(request.IsPaid ? "paid" : "unpaid")} successfully"));
    }
}
