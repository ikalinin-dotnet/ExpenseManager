using ExpenseManager.Application.Expenses.Commands.CreateExpense;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ExpenseManager.Application.Tests.Expenses.Validators;

public class CreateExpenseCommandValidatorTests
{
    private readonly CreateExpenseCommandValidator _validator = new();

    [Fact]
    public void ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new CreateExpenseCommand("Lunch", "Team lunch", 25.50m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldFail_WhenTitleIsEmpty()
    {
        var command = new CreateExpenseCommand("", null, 25m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void ShouldFail_WhenTitleExceedsMaxLength()
    {
        var command = new CreateExpenseCommand(new string('x', 201), null, 25m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void ShouldFail_WhenAmountIsZero()
    {
        var command = new CreateExpenseCommand("Test", null, 0m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldFail_WhenAmountIsNegative()
    {
        var command = new CreateExpenseCommand("Test", null, -5m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldFail_WhenCurrencyIsEmpty()
    {
        var command = new CreateExpenseCommand("Test", null, 25m, "", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void ShouldFail_WhenCurrencyIsWrongLength()
    {
        var command = new CreateExpenseCommand("Test", null, 25m, "US", DateTime.UtcNow, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void ShouldFail_WhenCategoryIdIsEmpty()
    {
        var command = new CreateExpenseCommand("Test", null, 25m, "USD", DateTime.UtcNow, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
