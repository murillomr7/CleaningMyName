using MediatR;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Domain.Interfaces.Repositories;

namespace CleaningMyName.Application.Debts.Queries.GetDebtsByUserId;

public record GetDebtsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<DebtDto>>;

public class GetDebtsByUserIdQueryHandler : IRequestHandler<GetDebtsByUserIdQuery, IEnumerable<DebtDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetDebtsByUserIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<DebtDto>> Handle(GetDebtsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            throw new ForbiddenAccessException();
        }

        var isAdmin = user.IsInRole("Admin");
        
        var currentUserId = user.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        if (!isAdmin && currentUserId != request.UserId.ToString())
        {
            throw new ForbiddenAccessException();
        }

        var debts = await _unitOfWork.DebtRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<IEnumerable<DebtDto>>(debts);
    }
}
