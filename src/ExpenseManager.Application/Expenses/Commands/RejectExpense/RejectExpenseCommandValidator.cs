using FluentValidation;

namespace ExpenseManager.Application.Expenses.Commands.RejectExpense;

public sealed class RejectExpenseCommandValidator : AbstractValidator<RejectExpenseCommand>
{
    public RejectExpenseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");
    }
}
