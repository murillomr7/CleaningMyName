using System.Security.Claims;
using CleaningMyName.Api.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace CleaningMyName.Api.Security.Handlers;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        // Get the user's date of birth claim
        var dateOfBirthClaim = context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth);

        // If no date of birth claim was found, fail
        if (dateOfBirthClaim == null)
        {
            return Task.CompletedTask;
        }

        // Parse the date of birth
        if (DateTime.TryParse(dateOfBirthClaim.Value, out var dateOfBirth))
        {
            // Calculate the user's age
            var age = DateTime.Today.Year - dateOfBirth.Year;
            
            // Check if the birthday has occurred this year
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            // If the user meets the minimum age, succeed
            if (age >= requirement.MinimumAge)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
