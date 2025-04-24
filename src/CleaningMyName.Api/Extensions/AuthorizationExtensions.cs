using Microsoft.AspNetCore.Authorization;

namespace CleaningMyName.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => 
                policy.RequireRole("Admin"));
            
            options.AddPolicy("RequireUserRole", policy => 
                policy.RequireRole("User"));

            options.AddPolicy("CanManageUsers", policy => 
                policy.RequireClaim("Permission", "users.manage"));
            
            options.AddPolicy("CanReadUsers", policy => 
                policy.RequireClaim("Permission", "users.read"));

            options.AddPolicy("FullAccess", policy => 
                policy.RequireRole("Admin")
                     .RequireClaim("Permission", "full.access"));

            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
