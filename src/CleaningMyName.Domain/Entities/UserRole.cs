using CleaningMyName.Domain.Common;

namespace CleaningMyName.Domain.Entities;

public class UserRole : BaseEntity
{
    private UserRole() { } // For EF Core

    public UserRole(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
}
