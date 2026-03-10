using ExpenseManager.Application.Expenses.Commands.RejectExpense;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ExpenseManager.Application.Tests.Expenses.Validators;

public class RejectExpenseCommandValidatorTests
{
    private readonly RejectExpenseCommandValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new RejectExpenseCommand(Guid.NewGuid(), "Over budget");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenIdIsEmpty()
    {
        var command = new RejectExpenseCommand(Guid.Empty, "Reason");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void ShouldFail_WhenReasonIsEmpty()
    {
        var command = new RejectExpenseCommand(Guid.NewGuid(), "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void ShouldFail_WhenReasonExceedsMaxLength()
    {
        var command = new RejectExpenseCommand(Guid.NewGuid(), new string('x', 501));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
