using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Application.Common.Models;
using MediatR;

namespace CleaningMyName.Application.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AuthenticationResult>>
{
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<Result<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Result.Failure<AuthenticationResult>(result.Message);
        }

        return Result.Success(result);
    }
}
