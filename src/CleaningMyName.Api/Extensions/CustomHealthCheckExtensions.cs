using CleaningMyName.Api.HealthChecks;
using CleaningMyName.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleaningMyName.Api.Extensions;

public static class CustomHealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        healthChecksBuilder.AddSqlServer(
            connectionString!, 
            name: "sqlserver",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql", "sqlserver" });

        healthChecksBuilder.AddProcessAllocatedMemoryHealthCheck(
            maximumMegabytesAllocated: 1024,
            name: "process-memory",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "memory" });

        healthChecksBuilder.AddDiskStorageHealthCheck(
            setup: options => options.AddDrive(Path.GetPathRoot(Directory.GetCurrentDirectory())!, 1024),
            name: "disk-space",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "storage" });

        healthChecksBuilder.AddCheck(
            "api", 
            () => HealthCheckResult.Healthy("API is running."),
            tags: new[] { "service" });
            
        services.AddScoped<UserServiceHealthCheck>();
        
        healthChecksBuilder.AddCheck<UserServiceHealthCheck>(
            "user-service",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "service", "business" });
            
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(60);
            options.MaximumHistoryEntriesPerEndpoint(50);
            options.SetApiMaxActiveRequests(1);
            
            options.AddHealthCheckEndpoint("API", "/health");
        })
        .AddInMemoryStorage();

        return services;
    }
}
