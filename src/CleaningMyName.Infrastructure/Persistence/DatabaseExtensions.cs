using CleaningMyName.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleaningMyName.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        try
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<DbInitializer>();
            
            logger.LogInformation("Migrating database...");
            
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
            
            var initializer = new DbInitializer(context, logger, passwordService);
            await initializer.InitializeAsync();
            
            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            var methodLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
            methodLogger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}
