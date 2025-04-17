using CleaningMyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleaningMyName.Infrastructure.Persistence.Configurations;

public class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(d => d.DueDate)
            .IsRequired();

        builder.Property(d => d.IsPaid)
            .HasDefaultValue(false);

        builder.Property(d => d.PaidOnUtc)
            .IsRequired(false);

        // Configure relationship with User
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
