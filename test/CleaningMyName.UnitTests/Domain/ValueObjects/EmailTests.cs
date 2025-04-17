using CleaningMyName.Domain.Exceptions;
using CleaningMyName.Domain.ValueObjects;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace CleaningMyName.UnitTests.Domain.ValueObjects;

public class EmailTests : TestBase
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("first.last@subdomain.domain.co.uk")]
    public void Create_WithValidEmail_ShouldCreateEmailObject(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullEmail_ShouldThrowDomainValidationException(string invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@dot")]
    [InlineData("@missinguser.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowDomainValidationException(string invalidEmail)
    {
        // Act
        Action act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is invalid.");
    }

    [Fact]
    public void Create_WithTooLongEmail_ShouldThrowDomainValidationException()
    {
        // Arrange
        var longLocalPart = new string('a', 320) + "@example.com";

        // Act
        Action act = () => Email.Create(longLocalPart);

        // Assert
        act.Should().Throw<DomainValidationException>()
            .WithMessage("Email is too long.");
    }

    [Fact]
    public void ImplicitConversion_FromEmailToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = Email.Create(emailValue);

        // Act
        string result = email;

        // Assert
        result.Should().Be(emailValue);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = Email.Create(emailValue);

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be(emailValue);
    }
}
