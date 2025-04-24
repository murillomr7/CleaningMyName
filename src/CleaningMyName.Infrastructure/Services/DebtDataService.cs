using CleaningMyName.Application.Debts;
using CleaningMyName.Application.Debts.Processing;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CleaningMyName.Infrastructure.Services;

public interface IDebtDataService
{
    Task<DebtSummaryDto?> GetUserDebtSummaryAsync(Guid userId, bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<PagedDebtResult<DebtDto>> GetPagedUserDebtsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DebtProcessingResult?> GetSystemDebtProcessingResultAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task InvalidateUserCacheAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class DebtDataService : IDebtDataService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    
    private static readonly Func<ApplicationDbContext, Guid, int, int, Task<PagedDebtResult<Debt>>> GetPagedDebtsQuery =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, Guid userId, int skip, int take) => 
                new PagedDebtResult<Debt>
                {
                    Items = context.Debts
                        .Where(d => d.UserId == userId)
                        .OrderByDescending(d => d.DueDate)
                        .Skip(skip)
                        .Take(take)
                        .Include(d => d.User)
                        .AsNoTracking()
                        .ToList(),
                    TotalCount = context.Debts.Count(d => d.UserId == userId),
                    PageSize = take,
                    PageNumber = (skip / take) + 1
                });

    public DebtDataService(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<DebtSummaryDto?> GetUserDebtSummaryAsync(Guid userId, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"debt_summary_user_{userId}";
        
        // Try to get from cache first (unless forceRefresh is true)
        if (!forceRefresh)
        {
            var cachedSummary = await _cacheService.GetAsync<DebtSummaryDto>(cacheKey, cancellationToken);
            if (cachedSummary != null)
            {
                return cachedSummary;
            }
        }
        
        // If not in cache or forced refresh
        var processingResult = await GetSystemDebtProcessingResultAsync(forceRefresh, cancellationToken);
        if (processingResult != null)
        {
            var userSummary = processingResult.UserSummaries.FirstOrDefault(s => s.UserId == userId);
            if (userSummary != null)
            {
                await _cacheService.SetAsync(cacheKey, userSummary, TimeSpan.FromHours(3), cancellationToken);
                return userSummary;
            }
        }
        
        return await CalculateUserDebtSummaryAsync(userId, cancellationToken);
    }

    public async Task<PagedDebtResult<DebtDto>> GetPagedUserDebtsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        int skip = (pageNumber - 1) * pageSize;
        
        var pagedResult = await GetPagedDebtsQuery(_dbContext, userId, skip, pageSize);
        
        return new PagedDebtResult<DebtDto>
        {
            Items = pagedResult.Items.Select(d => new DebtDto
            {
                Id = d.Id,
                UserId = d.UserId,
                UserName = d.User?.FullName ?? string.Empty,
                Description = d.Description,
                Amount = d.Amount,
                DueDate = d.DueDate,
                IsPaid = d.IsPaid,
                PaidOnUtc = d.PaidOnUtc,
                CreatedOnUtc = d.CreatedOnUtc,
                ModifiedOnUtc = d.ModifiedOnUtc
            }).ToList(),
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<DebtProcessingResult?> GetSystemDebtProcessingResultAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = "debt_processing_result";
        
        if (!forceRefresh)
        {
            var cachedResult = await _cacheService.GetAsync<DebtProcessingResult>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }
        
        return null;
    }

    public async Task InvalidateUserCacheAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync($"debt_summary_user_{userId}", cancellationToken);
        await _cacheService.RemoveAsync("debt_processing_result", cancellationToken);
    }

    private async Task<DebtSummaryDto?> CalculateUserDebtSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.FullName })
            .FirstOrDefaultAsync(cancellationToken);
            
        if (user == null)
            return null;
            
        var currentDate = DateTime.UtcNow.Date;
        
        var userDebts = await _dbContext.Debts
            .Where(d => d.UserId == userId)
            .ToListAsync(cancellationToken);
            
        var userOverdueDebts = userDebts
            .Where(d => !d.IsPaid && d.DueDate < currentDate)
            .ToList();
            
        var summary = new DebtSummaryDto
        {
            UserId = user.Id,
            UserName = user.FullName,
            TotalDebt = userDebts.Sum(d => d.Amount),
            TotalDebts = userDebts.Count,
            PaidDebts = userDebts.Count(d => d.IsPaid),
            OverdueDebts = userOverdueDebts.Count,
            OverdueAmount = userOverdueDebts.Sum(d => d.Amount),
            ProcessedAtUtc = DateTime.UtcNow
        };
        
        await _cacheService.SetAsync(
            $"debt_summary_user_{userId}", 
            summary,
            TimeSpan.FromHours(1),
            cancellationToken);
            
        return summary;
    }
}
