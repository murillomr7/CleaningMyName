using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Application.Common.Models;
using CleaningMyName.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CleaningMyName.Application.Debts.Commands.MarkDebtAsPaid;

public record MarkDebtAsPaidCommand(Guid Id, bool IsPaid) : IRequest<Result<Unit>>;

public class MarkDebtAsPaidCommandHandler : IRequestHandler<MarkDebtAsPaidCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MarkDebtAsPaidCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<Unit>> Handle(MarkDebtAsPaidCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.IsInRole("Admin"))
            {
                return Result.Failure<Unit>("Only administrators can mark debts as paid.");
            }

            var debt = await _unitOfWork.DebtRepository.GetByIdAsync(request.Id, cancellationToken);
            if (debt == null)
            {
                throw new NotFoundException("Debt", request.Id);
            }

            if (request.IsPaid)
            {
                debt.MarkAsPaid();
            }
            else
            {
                debt.MarkAsUnpaid();
            }

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
            return Result.Failure<Unit>($"Failed to update debt status: {ex.Message}");
        }
    }
}
