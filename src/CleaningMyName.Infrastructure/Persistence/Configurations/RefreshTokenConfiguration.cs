using CleaningMyName.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleaningMyName.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();

        builder.Property(rt => rt.IsUsed)
            .HasDefaultValue(false);

        builder.Property(rt => rt.IsRevoked)
            .HasDefaultValue(false);

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.Token);
    }
}
