using CleaningMyName.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CleaningMyName.Infrastructure.Persistence;

// This is a partial class to add RefreshToken to the DbContext
public partial class ApplicationDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
