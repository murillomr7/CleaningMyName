using CleaningMyName.Api.Models.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace CleaningMyName.Api.SwaggerExamples.Responses;

public class AuthResponseExample : IExamplesProvider<ApiResponse<AuthResponse>>
{
    public ApiResponse<AuthResponse> GetExamples()
    {
        return new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Authentication successful",
            Data = new AuthResponse
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = "John Doe",
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                RefreshToken = "NTJjY2YwZGUtMmMzMi00YmQwLWIzN2ItNTM0YTA5ZGYzNGVj",
                Roles = new List<string> { "User" },
                Expiration = DateTime.UtcNow.AddHours(1)
            }
        };
    }
}
