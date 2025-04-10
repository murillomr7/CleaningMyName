using CleaningMyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleaningMyName.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => ur.Id);

        // Ensure each user can have a role only once
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
    }
}
