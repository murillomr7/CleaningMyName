using CleaningMyName.Application.Common.Interfaces;
using CleaningMyName.Application.Common.Models;
using CleaningMyName.Application.Users.Commands.CreateUser;
using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.Interfaces.Repositories;
using CleaningMyName.Domain.ValueObjects;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace CleaningMyName.UnitTests.Application.Users.Commands;

public class CreateUserCommandTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;

    public CreateUserCommandTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordService = new Mock<IPasswordService>();

        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(u => u.RoleRepository).Returns(_mockRoleRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserAndReturnSuccess()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            Roles = new List<string> { "User" }
        };

        var hashedPassword = "hashedPassword123";
        _mockPasswordService.Setup(s => s.HashPassword(command.Password)).Returns(hashedPassword);
        
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), default))
            .ReturnsAsync((User)null);
        
        var userRole = new Role("User", "Standard user role");
        _mockRoleRepository.Setup(r => r.GetByNameAsync("User", default))
            .ReturnsAsync(userRole);

        User capturedUser = null;
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .ReturnsAsync((User u, CancellationToken _) => u);

        var handler = new CreateUserCommandHandler(_mockUnitOfWork.Object, _mockPasswordService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        capturedUser.Should().NotBeNull();
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.Email.Value.Should().Be(command.Email);
        capturedUser.PasswordHash.Should().Be(hashedPassword);
        
        capturedUser.UserRoles.Should().HaveCount(1);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Password = "Password123!",
            Roles = new List<string>()
        };

        var existingUser = new User(
            "Existing", 
            "User", 
            Email.Create(command.Email), 
            "existingHash");
        
        _mockUserRepository.Setup(r => r.GetByEmailAsync(It.Is<Email>(e => e.Value == command.Email), default))
            .ReturnsAsync(existingUser);

        var handler = new CreateUserCommandHandler(_mockUnitOfWork.Object, _mockPasswordService.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }
}
