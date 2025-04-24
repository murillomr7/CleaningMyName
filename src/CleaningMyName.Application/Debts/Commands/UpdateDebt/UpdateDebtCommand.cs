using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Application.Common.Models;
using CleaningMyName.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CleaningMyName.Application.Debts.Commands.UpdateDebt;

public record UpdateDebtCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
}

public class UpdateDebtCommandHandler : IRequestHandler<UpdateDebtCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateDebtCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<Unit>> Handle(UpdateDebtCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.IsInRole("Admin"))
            {
                return Result.Failure<Unit>("Only administrators can update debts.");
            }

            var debt = await _unitOfWork.DebtRepository.GetByIdAsync(request.Id, cancellationToken);
            if (debt == null)
            {
                throw new NotFoundException("Debt", request.Id);
            }

            debt.UpdateDetails(
                request.Description,
                request.Amount,
                request.DueDate);

            await _unitOfWork.DebtRepository.UpdateAsync(debt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (NotFoundException)
        {
            return Result.Failure<Unit>($"Debt with ID {request.Id} not found.");
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>($"Failed to update debt: {ex.Message}");
        }
    }
}
