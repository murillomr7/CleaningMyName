using CleaningMyName.Application.Debts.Processing;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace CleaningMyName.Infrastructure.BackgroundServices;

public class DebtProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DebtProcessingService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(30);
    
    // Compiled query for better performance on frequent executions
    private static readonly Func<ApplicationDbContext, DateTime, IAsyncEnumerable<Debt>> GetOverdueDebtsQuery = 
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, DateTime currentDate) => 
                context.Debts
                    .Where(d => !d.IsPaid && d.DueDate < currentDate)
                    .Include(d => d.User)
        );

    public DebtProcessingService(
        IServiceScopeFactory scopeFactory,
        ILogger<DebtProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Debt Processing Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Debt Processing Service is running the processing task.");
            
            try
            {
                await ProcessDebtsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing debts.");
            }

            _logger.LogInformation("Debt Processing Service is going to sleep for {Interval}.", _processingInterval);
            await Task.Delay(_processingInterval, stoppingToken);
        }
    }

    private async Task ProcessDebtsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        
        // Process debt data
        var result = await CalculateDebtSummaries(dbContext, stoppingToken);
        
        // Cache the results
        await cacheService.SetAsync(
            "debt_processing_result", 
            result,
            TimeSpan.FromHours(6), 
            stoppingToken);
        
        // Cache individual user summaries for quicker access
        foreach (var userSummary in result.UserSummaries)
        {
            await cacheService.SetAsync(
                $"debt_summary_user_{userSummary.UserId}", 
                userSummary,
                TimeSpan.FromHours(3), 
                stoppingToken);
        }

        _logger.LogInformation("Processed and cached debt data: {TotalDebts} debts across {UserCount} users.",
            result.TotalSystemDebts, result.UserSummaries.Count);
    }

    private async Task<DebtProcessingResult> CalculateDebtSummaries(ApplicationDbContext dbContext, CancellationToken stoppingToken)
    {
        var currentDate = DateTime.UtcNow.Date;
        var result = new DebtProcessingResult
        {
            ProcessedAtUtc = DateTime.UtcNow
        };

        // Project users and include only needed properties
        var users = await dbContext.Users
            .Select(u => new
            {
                u.Id,
                u.FullName
            })
            .ToListAsync(stoppingToken);

        result.UserSummaries = new List<DebtSummaryDto>(users.Count);
        
        // Group debts by user and calculate summaries
        var allDebts = await dbContext.Debts
            .Include(d => d.User)
            .AsNoTracking()
            .ToListAsync(stoppingToken);

        result.TotalSystemDebts = allDebts.Count;
        result.TotalSystemDebt = allDebts.Sum(d => d.Amount);
        
        // Get overdue debts using compiled query for better performance
        var overdueDebts = new List<Debt>();
        await foreach (var debt in GetOverdueDebtsQuery(dbContext, currentDate).WithCancellation(stoppingToken))
        {
            overdueDebts.Add(debt);
        }
        
        result.TotalSystemOverdueDebts = overdueDebts.Count;

        foreach (var user in users)
        {
            var userDebts = allDebts.Where(d => d.UserId == user.Id).ToList();
            var userOverdueDebts = overdueDebts.Where(d => d.UserId == user.Id).ToList();
            
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
            
            result.UserSummaries.Add(summary);
        }

        return result;
    }
}
