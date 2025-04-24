using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CleaningMyName.Infrastructure.Persistence.Repositories;

public class DebtRepository : RepositoryBase<Debt>, IDebtRepository
{
    private readonly IDebtDataService? _debtDataService;

    public DebtRepository(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        : base(dbContext)
    {
        // Use try/catch to avoid circular dependency issues
        try
        {
            _debtDataService = serviceProvider.GetService(typeof(IDebtDataService)) as IDebtDataService;
        }
        catch
        {
            // Ignore - the service might not be registered yet during startup
        }
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

    public override async Task<Debt> AddAsync(Debt entity, CancellationToken cancellationToken = default)
    {
        var result = await base.AddAsync(entity, cancellationToken);
        await InvalidateUserCacheAsync(entity.UserId, cancellationToken);
        return result;
    }

    public override async Task UpdateAsync(Debt entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await InvalidateUserCacheAsync(entity.UserId, cancellationToken);
    }

    public override async Task DeleteAsync(Debt entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await InvalidateUserCacheAsync(entity.UserId, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (_debtDataService != null)
        {
            await _debtDataService.InvalidateUserCacheAsync(userId, cancellationToken);
        }
    }
}
