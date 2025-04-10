using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CleaningMyName.Infrastructure.Persistence.Repositories;

public class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public override async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .AnyAsync(r => r.Name == name, cancellationToken);
    }
}
