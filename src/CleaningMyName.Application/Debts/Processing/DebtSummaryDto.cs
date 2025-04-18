namespace CleaningMyName.Application.Debts.Processing;

public class DebtSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public int TotalDebts { get; set; }
    public int PaidDebts { get; set; }
    public int OverdueDebts { get; set; }
    public decimal OverdueAmount { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}

public class DebtProcessingResult
{
    public List<DebtSummaryDto> UserSummaries { get; set; } = new();
    public decimal TotalSystemDebt { get; set; }
    public int TotalSystemDebts { get; set; }
    public int TotalSystemOverdueDebts { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}

public class PagedDebtResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
