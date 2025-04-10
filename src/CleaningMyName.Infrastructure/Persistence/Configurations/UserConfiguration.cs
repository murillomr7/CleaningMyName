using CleaningMyName.Domain.Entities;
using CleaningMyName.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleaningMyName.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        // Configure Email value object using a value converter
        builder.Property(u => u.Email)
            .HasConversion(new EmailValueConverter())
            .HasMaxLength(320)
            .IsRequired();

        // Create a unique index on the Email property
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        // Configure relationship with UserRole join entity
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

