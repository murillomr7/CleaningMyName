namespace CleaningMyName.Domain.Interfaces.Repositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
