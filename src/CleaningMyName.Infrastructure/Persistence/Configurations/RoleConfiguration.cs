using CleaningMyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleaningMyName.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Role("Admin", "Administrator with full access") { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), CreatedOnUtc = DateTime.UtcNow },
            new Role("User", "Regular user with limited access") { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), CreatedOnUtc = DateTime.UtcNow }
        );
    }
}
