using CleaningMyName.Domain.Entities;

namespace CleaningMyName.Domain.Interfaces.Repositories;

public interface IDebtRepository
{
    Task<Debt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Debt>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Debt>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Debt> AddAsync(Debt debt, CancellationToken cancellationToken = default);
    Task UpdateAsync(Debt debt, CancellationToken cancellationToken = default);
    Task DeleteAsync(Debt debt, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
