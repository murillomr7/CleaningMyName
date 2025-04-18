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
        // Configure database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Configure Redis caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "CleaningMyName:";
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IDebtRepository, DebtRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDebtDataService, DebtDataService>();

        // Register caching
        services.AddSingleton<ICacheService, RedisCacheService>();

        // Register background service
        services.AddHostedService<DebtProcessingService>();

        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        // Configure JWT
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<JwtTokenGenerator>();

        return services;
    }
}
