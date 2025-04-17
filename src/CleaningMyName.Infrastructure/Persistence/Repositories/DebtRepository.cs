using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CleaningMyName.Infrastructure.Persistence.Repositories;

public class DebtRepository : RepositoryBase<Debt>, IDebtRepository
{
    public DebtRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task<Debt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Debt>()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Debt>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Debt>()
            .Include(d => d.User)
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IEnumerable<Debt>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Debt>()
            .Include(d => d.User)
            .ToListAsync(cancellationToken);
    }
}
