using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleaningMyName.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
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

    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
        // Main health check endpoint - returns detailed health status
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Ready health check endpoint - checks if the application is ready to accept requests
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("service") || check.Tags.Contains("db"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Live health check endpoint - checks if the application is running
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("service"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Add health check UI dashboard
        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });

        return app;
    }
}
