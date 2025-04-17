using CleaningMyName.Domain.Entities;
using CleaningMyName.Domain.ValueObjects;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace CleaningMyName.UnitTests.Domain.Entities;

public class UserTests : TestBase
{
    private readonly Email _validEmail = Email.Create("test@example.com");
    private readonly string _passwordHash = "hashedPassword123";
    
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserWithCorrectProperties()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = new User(firstName, lastName, _validEmail, _passwordHash);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(_validEmail);
        user.PasswordHash.Should().Be(_passwordHash);
        user.CreatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.IsActive.Should().BeTrue();
        user.LastLoginUtc.Should().BeNull();
        user.ModifiedOnUtc.Should().BeNull();
        user.UserRoles.Should().BeEmpty();
        user.FullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void UpdateName_ShouldUpdateNameAndModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateName(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.FullName.Should().Be($"{newFirstName} {newLastName}");
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmailAndModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var newEmail = Email.Create("new@example.com");

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordAndModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var newPasswordHash = "newHashedPassword456";

        // Act
        user.UpdatePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddRole_ShouldAddRoleToUserRoles()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var role = new Role("Admin", "Administrator role");

        // Act
        user.AddRole(role);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.UserRoles.First().RoleId.Should().Be(role.Id);
        user.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void AddRole_WhenRoleAlreadyExists_ShouldNotAddDuplicate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var role = new Role("Admin", "Administrator role");

        // Act
        user.AddRole(role);
        user.AddRole(role); // Try to add the same role again

        // Assert
        user.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveRole_WhenRoleExists_ShouldRemoveFromUserRoles()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        var role = new Role("Admin", "Administrator role");
        user.AddRole(role);

        // Act
        user.RemoveRole(role);

        // Assert
        user.UserRoles.Should().BeEmpty();
        user.ModifiedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAndModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginUtc.Should().NotBeNull();
        user.LastLoginUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndUpdateModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrueAndUpdateModifiedDate()
    {
        // Arrange
        var user = new User("John", "Doe", _validEmail, _passwordHash);
        user.Deactivate(); // First deactivate

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.ModifiedOnUtc.Should().NotBeNull();
        user.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
