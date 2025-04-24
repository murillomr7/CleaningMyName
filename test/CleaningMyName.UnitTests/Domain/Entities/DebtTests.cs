using CleaningMyName.Domain.Entities;
using CleaningMyName.UnitTests.Common;
using FluentAssertions;

namespace CleaningMyName.UnitTests.Domain.Entities;

public class DebtTests : TestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDebtWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var description = "Test Debt";
        var amount = 100m;
        var dueDate = DateTime.Today.AddDays(30);
        var isPaid = false;

        // Act
        var debt = new Debt(userId, description, amount, dueDate, isPaid);

        // Assert
        debt.Id.Should().NotBeEmpty();
        debt.UserId.Should().Be(userId);
        debt.Description.Should().Be(description);
        debt.Amount.Should().Be(amount);
        debt.DueDate.Should().Be(dueDate);
        debt.IsPaid.Should().Be(isPaid);
        debt.CreatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        debt.PaidOnUtc.Should().BeNull();
        debt.ModifiedOnUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdatePropertiesAndModifiedDate()
    {
        // Arrange
        var debt = new Debt(Guid.NewGuid(), "Original Description", 100m, DateTime.Today.AddDays(30));
        var newDescription = "Updated Description";
        var newAmount = 200m;
        var newDueDate = DateTime.Today.AddDays(60);

        // Act
        debt.UpdateDetails(newDescription, newAmount, newDueDate);

        // Assert
        debt.Description.Should().Be(newDescription);
        debt.Amount.Should().Be(newAmount);
        debt.DueDate.Should().Be(newDueDate);
        debt.ModifiedOnUtc.Should().NotBeNull();
        debt.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsPaid_WhenNotPaid_ShouldSetIsPaidToTrueAndUpdateDates()
    {
        // Arrange
        var debt = new Debt(Guid.NewGuid(), "Test Debt", 100m, DateTime.Today.AddDays(30), false);

        // Act
        debt.MarkAsPaid();

        // Assert
        debt.IsPaid.Should().BeTrue();
        debt.PaidOnUtc.Should().NotBeNull();
        debt.PaidOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        debt.ModifiedOnUtc.Should().NotBeNull();
        debt.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldNotChangeAnything()
    {
        // Arrange
        var debt = new Debt(Guid.NewGuid(), "Test Debt", 100m, DateTime.Today.AddDays(30), true);
        debt.MarkAsPaid();
        var originalPaidOnUtc = debt.PaidOnUtc;
        var originalModifiedOnUtc = debt.ModifiedOnUtc;

        System.Threading.Thread.Sleep(10);

        // Act
        debt.MarkAsPaid();

        // Assert
        debt.IsPaid.Should().BeTrue();
        debt.PaidOnUtc.Should().Be(originalPaidOnUtc);
        debt.ModifiedOnUtc.Should().Be(originalModifiedOnUtc);
    }

    [Fact]
    public void MarkAsUnpaid_WhenPaid_ShouldSetIsPaidToFalseAndClearPaidDate()
    {
        // Arrange
        var debt = new Debt(Guid.NewGuid(), "Test Debt", 100m, DateTime.Today.AddDays(30), true);
        debt.MarkAsPaid();

        // Act
        debt.MarkAsUnpaid();

        // Assert
        debt.IsPaid.Should().BeFalse();
        debt.PaidOnUtc.Should().BeNull();
        debt.ModifiedOnUtc.Should().NotBeNull();
        debt.ModifiedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsUnpaid_WhenAlreadyUnpaid_ShouldNotChangeAnything()
    {
        // Arrange
        var debt = new Debt(Guid.NewGuid(), "Test Debt", 100m, DateTime.Today.AddDays(30), false);
        debt.ModifiedOnUtc = DateTime.UtcNow.AddHours(-1); 
        var originalModifiedOnUtc = debt.ModifiedOnUtc;

        // Act
        debt.MarkAsUnpaid();

        // Assert
        debt.IsPaid.Should().BeFalse();
        debt.PaidOnUtc.Should().BeNull();
        debt.ModifiedOnUtc.Should().Be(originalModifiedOnUtc);
    }
}
