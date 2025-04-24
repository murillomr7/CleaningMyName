using CleaningMyName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace CleaningMyName.Api.Extensions;

public static class DatabaseStartupHelpers
{
    public static async Task WaitForDatabaseAsync(this WebApplication app, int retries = 10)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Waiting for database to be ready...");
        
        var retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Database connection attempt {RetryCount} failed after {TimeSpan:g}. Error: {Error}",
                        retryCount,
                        timeSpan,
                        exception.Message);
                });
                
        await retry.ExecuteAsync(async () =>
        {
            // Create scope to resolve scoped DbContext
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Test connection
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.CloseConnectionAsync();
            
            logger.LogInformation("Database connection established");
        });
    }
}
