namespace CleaningMyName.Api.Models.Requests.Debts;

public class CreateDebtRequest
{
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
}
