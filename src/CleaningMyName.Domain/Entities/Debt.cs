using CleaningMyName.Domain.Common;

namespace CleaningMyName.Domain.Entities;

public class Debt : BaseEntity
{
    private Debt() { } 

    public Debt(
        Guid userId,
        string description,
        decimal amount,
        DateTime dueDate,
        bool isPaid = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        IsPaid = isPaid;
        CreatedOnUtc = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidOnUtc { get; private set; }

    public void UpdateDetails(string description, decimal amount, DateTime dueDate)
    {
        Description = description;
        Amount = amount;
        DueDate = dueDate;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (!IsPaid)
        {
            IsPaid = true;
            PaidOnUtc = DateTime.UtcNow;
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    public void MarkAsUnpaid()
    {
        if (IsPaid)
        {
            IsPaid = false;
            PaidOnUtc = null;
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }
}
