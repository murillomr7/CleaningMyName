using AutoMapper;
using CleaningMyName.Application.Common.Exceptions;
using CleaningMyName.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CleaningMyName.Application.Debts.Queries.GetAllDebts;

public record GetAllDebtsQuery : IRequest<IEnumerable<DebtDto>>;

public class GetAllDebtsQueryHandler : IRequestHandler<GetAllDebtsQuery, IEnumerable<DebtDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetAllDebtsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<DebtDto>> Handle(GetAllDebtsQuery request, CancellationToken cancellationToken)
    {
        // Check if user is admin
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException();
        }

        var debts = await _unitOfWork.DebtRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<DebtDto>>(debts);
    }
}
