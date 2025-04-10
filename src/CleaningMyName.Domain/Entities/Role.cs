using CleaningMyName.Domain.Common;

namespace CleaningMyName.Domain.Entities;

public class Role : BaseEntity
{
    private readonly List<UserRole> _userRoles = new();
    
    private Role() { } // For EF Core
    
    public Role(string name, string description = "")
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedOnUtc = DateTime.UtcNow;
    }
    
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    
    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
