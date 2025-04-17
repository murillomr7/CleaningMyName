namespace CleaningMyName.Api.Models.Requests.Debts;

public class UpdateDebtRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}
