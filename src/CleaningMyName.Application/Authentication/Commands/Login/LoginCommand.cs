using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Application.Common.Models;
using MediatR;

namespace CleaningMyName.Application.Authentication.Commands.Login;

public record LoginCommand : IRequest<Result<AuthenticationResult>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.AuthenticateAsync(request.Email, request.Password);

        if (!result.Success)
        {
            return Result.Failure<AuthenticationResult>(result.Message);
        }

        return Result.Success(result);
    }
}
