using CleaningMyName.Application.Common.Models;

namespace CleaningMyName.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string email, string password);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken);
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
