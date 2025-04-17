using CleaningMyName.Api.Models.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace CleaningMyName.Api.SwaggerExamples.Requests;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "user@example.com",
            Password = "P4ssw0rd#$@!366_!@^$"
        };
    }
}
