using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleaningMyName.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IPasswordService _passwordService;

    public DbInitializer(
        ApplicationDbContext context,
        ILogger<DbInitializer> logger,
        IPasswordService passwordService)
    {
        _context = context;
        _logger = logger;
        _passwordService = passwordService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Apply migrations if they are not applied
            if (_context.Database.GetPendingMigrations().Any())
            {
                _logger.LogInformation("Applying migrations...");
                await _context.Database.MigrateAsync();
            }

            // Seed data
            await SeedDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        // Seed roles if none exist
        if (!await _context.Roles.AnyAsync())
        {
            _logger.LogInformation("Seeding roles...");
            
            var adminRole = new Role("Admin", "Administrator with full access");
            var userRole = new Role("User", "Regular user with limited access");
            
            _context.Roles.AddRange(adminRole, userRole);
            await _context.SaveChangesAsync();
        }

        // Seed admin user if none exist
        if (!await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Seeding admin user...");
            
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminPassword = _passwordService.HashPassword("Admin@123456");
                var adminUser = new User(
                    "Admin",
                    "User",
                    Email.Create("admin@cleaningmyname.com"),
                    adminPassword);
                
                // Add admin role to user using the UserRole join entity
                var userRoleEntity = new UserRole(adminUser.Id, adminRole.Id);
                adminUser.AddUserRole(userRoleEntity);
                
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
