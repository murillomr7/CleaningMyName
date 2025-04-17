using MediatR;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Domain.Interfaces.Repositories;

namespace CleaningMyName.Application.Debts.Queries.GetDebtById;

public record GetDebtByIdQuery(Guid Id) : IRequest<DebtDto>;

public class GetDebtByIdQueryHandler : IRequestHandler<GetDebtByIdQuery, DebtDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetDebtByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DebtDto> Handle(GetDebtByIdQuery request, CancellationToken cancellationToken)
    {
        var debt = await _unitOfWork.DebtRepository.GetByIdAsync(request.Id, cancellationToken);

        if (debt == null)
        {
            throw new NotFoundException("Debt", request.Id);
        }

        // Check if user has access
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new ForbiddenAccessException();
        }

        // Allow access if user is admin or the debt belongs to the current user
        var isAdmin = user.IsInRole("Admin");
        var userId = user.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!isAdmin && userId != debt.UserId.ToString())
        {
            throw new ForbiddenAccessException();
        }

        return _mapper.Map<DebtDto>(debt);
    }
}
