using FluentValidation;

namespace ExpenseManager.Application.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Expense ID is required.");
    }
}
