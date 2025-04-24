using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleaningMyName.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        healthChecksBuilder.AddSqlServer(
            connectionString!, 
            name: "sqlserver",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "db", "sql", "sqlserver" });

        healthChecksBuilder.AddProcessAllocatedMemoryHealthCheck(
            maximumMegabytesAllocated: 1024, // Maximum memory threshold (1GB)
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

    public static IApplicationBuilder UseCustomHealthChecks(this IApplicationBuilder app)
    {
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

        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });

        return app;
    }
}
