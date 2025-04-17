using CleaningMyName.Application.Common.Models;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CleaningMyName.Application.Debts.Commands.CreateDebt;

public record CreateDebtCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsPaid { get; init; }
}

public class CreateDebtCommandHandler : IRequestHandler<CreateDebtCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateDebtCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<Guid>> Handle(CreateDebtCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if current user is an admin
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.IsInRole("Admin"))
            {
                return Result.Failure<Guid>("Only administrators can create debts.");
            }

            // Verify user exists
            var userExists = await _unitOfWork.UserRepository.ExistsAsync(request.UserId, cancellationToken);
            if (!userExists)
            {
                return Result.Failure<Guid>($"User with ID {request.UserId} does not exist.");
            }

            // Create new debt
            var debt = new Debt(
                request.UserId,
                request.Description,
                request.Amount,
                request.DueDate,
                request.IsPaid);

            // Add to repository
            await _unitOfWork.DebtRepository.AddAsync(debt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(debt.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Failed to create debt: {ex.Message}");
        }
    }
}
