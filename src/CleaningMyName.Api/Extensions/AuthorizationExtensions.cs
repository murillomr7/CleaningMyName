using Microsoft.AspNetCore.Authorization;

namespace CleaningMyName.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy("RequireAdminRole", policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy("RequireUserRole", policy => 
                policy.RequireRole("User"));

            // Claim-based policies
            options.AddPolicy("CanManageUsers", policy => 
                policy.RequireClaim("Permission", "users.manage"));
            
            options.AddPolicy("CanReadUsers", policy => 
                policy.RequireClaim("Permission", "users.read"));

            // Combined policies
            options.AddPolicy("FullAccess", policy => 
                policy.RequireRole("Admin")
                     .RequireClaim("Permission", "full.access"));

            // Default policy - requires authentication
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Fallback policy - applied when no specific policy is specified
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
