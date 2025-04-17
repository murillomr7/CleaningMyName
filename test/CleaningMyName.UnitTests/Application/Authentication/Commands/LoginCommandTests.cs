using CleaningMyName.Application.Authentication.Commands.Login;
using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;
using Moq;

namespace CleaningMyName.UnitTests.Application.Authentication.Commands;

public class LoginCommandTests : TestBase
{
    private readonly Mock<IAuthenticationService> _mockAuthService;

    public LoginCommandTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var authResult = new AuthenticationResult
        {
            Success = true,
            UserId = "user123",
            UserName = "Test User",
            Token = "jwt-token",
            RefreshToken = "refresh-token",
            Roles = new List<string> { "User" }
        };

        _mockAuthService.Setup(s => s.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(authResult);

        var handler = new LoginCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(authResult);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "wrongpassword"
        };

        var authResult = new AuthenticationResult
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mockAuthService.Setup(s => s.AuthenticateAsync(command.Email, command.Password))
            .ReturnsAsync(authResult);

        var handler = new LoginCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(authResult.Message);
    }
}
