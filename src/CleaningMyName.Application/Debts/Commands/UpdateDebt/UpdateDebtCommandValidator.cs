using FluentValidation;

namespace CleaningMyName.Application.Debts.Commands.UpdateDebt;

public class UpdateDebtCommandValidator : AbstractValidator<UpdateDebtCommand>
{
    public UpdateDebtCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Debt ID is required.");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

        RuleFor(v => v.Amount)
            .NotEmpty().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(v => v.DueDate)
            .NotEmpty().WithMessage("Due date is required.");
    }
}
