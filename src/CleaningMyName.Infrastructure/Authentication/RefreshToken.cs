using CleaningMyName.Domain.Common;
using CleaningMyName.Domain.Entities;

namespace CleaningMyName.Infrastructure.Authentication;

public class RefreshToken : BaseEntity
{
    private RefreshToken() { } 

    public RefreshToken(Guid userId, string token, DateTime expiryDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiryDate = expiryDate;
        CreatedOnUtc = DateTime.UtcNow;
        IsRevoked = false;
        IsUsed = false;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsActive => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;

    public void MarkAsUsed()
    {
        IsUsed = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsRevoked = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }
}
