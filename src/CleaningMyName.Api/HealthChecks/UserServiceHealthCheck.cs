using CleaningMyName.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleaningMyName.Api.HealthChecks;

public class UserServiceHealthCheck : IHealthCheck
{
    private readonly IUserRepository _userRepository;

    public UserServiceHealthCheck(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            
            if (users.Any())
            {
                return HealthCheckResult.Healthy("User service is working correctly");
            }
            
            return HealthCheckResult.Degraded("No users found in the system");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("User service failed to respond", ex);
        }
    }
}
