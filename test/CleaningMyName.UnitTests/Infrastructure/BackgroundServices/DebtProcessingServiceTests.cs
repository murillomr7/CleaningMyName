using Moq;
using CleaningMyName.Domain.Entities;
using CleaningMyName.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleaningMyName.Infrastructure.BackgroundServices;
using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.Infrastructure.Persistence;

namespace CleaningMyName.UnitTests.Infrastructure.BackgroundServices;

public class DebtProcessingServiceTests : TestBase
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly Mock<DbSet<Debt>> _mockDebtDbSet;
    private readonly Mock<DbSet<User>> _mockUserDbSet;
    private readonly Mock<ILogger<DebtProcessingService>> _mockLogger;
    private readonly DebtProcessingService _service;

    public DebtProcessingServiceTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<DebtProcessingService>>();

        var options = new DbContextOptions<ApplicationDbContext>();
        _mockDbContext = new Mock<ApplicationDbContext>(options);
        
        _mockDebtDbSet = new Mock<DbSet<Debt>>();
        _mockUserDbSet = new Mock<DbSet<User>>();
        
        _mockDbContext.Setup(db => db.Debts).Returns(_mockDebtDbSet.Object);
        _mockDbContext.Setup(db => db.Users).Returns(_mockUserDbSet.Object);

        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ApplicationDbContext))).Returns(_mockDbContext.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(ICacheService))).Returns(_mockCacheService.Object);

        _service = new DebtProcessingService(_mockScopeFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleCancellation()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        
        await _service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        cancellationTokenSource.Cancel();
        await _service.StopAsync(CancellationToken.None);
        
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), 
            Times.AtLeastOnce);
    }
}
