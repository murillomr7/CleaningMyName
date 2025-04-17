using CleaningMyName.Api.HealthChecks;
using CleaningMyName.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleaningMyName.Api.Extensions;

public static class CustomHealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // Register health checks
        var healthChecksBuilder = services.AddHealthChecks();
        
        // Database health check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        healthChecksBuilder.AddSqlServer(
            connectionString!, 
            name: "sqlserver",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql", "sqlserver" });

        // Memory health check
        healthChecksBuilder.AddProcessAllocatedMemoryHealthCheck(
            maximumMegabytesAllocated: 1024, // Maximum memory threshold (1GB)
            name: "process-memory",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "memory" });

        // Disk storage health check
        healthChecksBuilder.AddDiskStorageHealthCheck(
            setup: options => options.AddDrive(Path.GetPathRoot(Directory.GetCurrentDirectory())!, 1024), // Minimum 1GB free space
            name: "disk-space",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "storage" });

        // Self health check
        healthChecksBuilder.AddCheck(
            "api", 
            () => HealthCheckResult.Healthy("API is running."),
            tags: new[] { "service" });
            
        // Register UserServiceHealthCheck
        services.AddScoped<UserServiceHealthCheck>();
        
        // Add custom user service health check
        healthChecksBuilder.AddCheck<UserServiceHealthCheck>(
            "user-service",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "service", "business" });
            
        // Add health check UI
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(60); // Evaluate every 60 seconds
            options.MaximumHistoryEntriesPerEndpoint(50); // Store 50 history entries
            options.SetApiMaxActiveRequests(1); // Set 1 active request at a time
            
            // Add health check endpoint to be monitored
            options.AddHealthCheckEndpoint("API", "/health");
        })
        .AddInMemoryStorage(); // Use in-memory storage for health check history

        return services;
    }
}
