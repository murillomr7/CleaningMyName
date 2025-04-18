using CleaningMyName.Application.Debts.Processing;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.Infrastructure.Persistence;
using CleaningMyName.Infrastructure.Services;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CleaningMyName.UnitTests.Infrastructure.Services;

public class DebtDataServiceTests : TestBase
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<DbSet<Debt>> _mockDebtDbSet;
    private readonly Mock<DbSet<User>> _mockUserDbSet;
    private readonly IDebtDataService _debtDataService;

    public DebtDataServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        
        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        
        _mockDebtDbSet = new Mock<DbSet<Debt>>();
        _mockUserDbSet = new Mock<DbSet<User>>();
        
        _mockDbContext.Setup(db => db.Debts).Returns(_mockDebtDbSet.Object);
        _mockDbContext.Setup(db => db.Users).Returns(_mockUserDbSet.Object);

        _debtDataService = new DebtDataService(_mockDbContext.Object, _mockCacheService.Object);
    }

    [Fact]
    public async Task GetUserDebtSummaryAsync_WhenCacheHasValue_ShouldReturnCachedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cacheKey = $"debt_summary_user_{userId}";
        var cachedSummary = new DebtSummaryDto
        {
            UserId = userId,
            UserName = "Test User",
            TotalDebt = 1000,
            TotalDebts = 5,
            PaidDebts = 2,
            OverdueDebts = 1,
            OverdueAmount = 100,
            ProcessedAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };

        _mockCacheService.Setup(x => x.GetAsync<DebtSummaryDto>(cacheKey, default))
            .ReturnsAsync(cachedSummary);

        // Act
        var result = await _debtDataService.GetUserDebtSummaryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedSummary);
        _mockCacheService.Verify(x => x.GetAsync<DebtSummaryDto>(cacheKey, default), Times.Once);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_ShouldRemoveCacheEntries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userSummaryKey = $"debt_summary_user_{userId}";
        var systemSummaryKey = "debt_processing_result";

        // Act
        await _debtDataService.InvalidateUserCacheAsync(userId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveAsync(userSummaryKey, default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(systemSummaryKey, default), Times.Once);
    }
}
