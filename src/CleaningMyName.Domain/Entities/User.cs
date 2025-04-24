using CleaningMyName.Domain.Common;
using CleaningMyName.Domain.ValueObjects;

namespace CleaningMyName.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<UserRole> _userRoles = new();

    private User() { }

    public User(
        string firstName, 
        string lastName, 
        Email email, 
        string passwordHash)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime? LastLoginUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        ModifiedOnUtc = DateTime.UtcNow;
    }
    
    public void UpdateEmail(Email email)
    {
        Email = email;
        ModifiedOnUtc = DateTime.UtcNow;
    }
    
    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        ModifiedOnUtc = DateTime.UtcNow;
    }
    
    public void AddRole(Role role)
    {
        var userRole = new UserRole(Id, role.Id);
        if (!_userRoles.Any(ur => ur.RoleId == role.Id))
        {
            _userRoles.Add(userRole);
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }
    
    
    public void AddUserRole(UserRole userRole)
    {
        if (!_userRoles.Any(ur => ur.RoleId == userRole.RoleId))
        {
            _userRoles.Add(userRole);
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }
    
    public void RemoveRole(Role role)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }
    
    public void RecordLogin()
    {
        LastLoginUtc = DateTime.UtcNow;
        ModifiedOnUtc = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
