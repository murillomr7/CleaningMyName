using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Infrastructure.Authentication;
using CleaningMyName.Infrastructure.BackgroundServices;
using CleaningMyName.Infrastructure.Caching;
using CleaningMyName.Infrastructure.Persistence;
using CleaningMyName.Infrastructure.Persistence.Repositories;
using CleaningMyName.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleaningMyName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((provider, options) => {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "CleaningMyName:";
            options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            {
                ConnectRetry = 5,
                ConnectTimeout = 5000
            };
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IDebtRepository, DebtRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDebtDataService, DebtDataService>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddHostedService<DebtProcessingService>();

        services.AddHttpContextAccessor();

        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<JwtTokenGenerator>();

        return services;
    }
}
