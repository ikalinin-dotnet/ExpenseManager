using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Exceptions;
using ExpenseManager.Domain.ValueObjects;
using FluentAssertions;

namespace ExpenseManager.Domain.Tests.Entities;

public class ExpenseTests
{
    private static Expense CreateDraftExpense(string userId = "user-1") =>
        new(Guid.NewGuid(), "Lunch", "Team lunch", new Money(25.50m), DateTime.UtcNow, Guid.NewGuid(), userId);

    [Fact]
    public void Constructor_ShouldCreateDraftExpense()
    {
        var expense = CreateDraftExpense();

        expense.Status.Should().Be(ExpenseStatus.Draft);
        expense.Title.Should().Be("Lunch");
        expense.Amount.Amount.Should().Be(25.50m);
        expense.ApproverId.Should().BeNull();
        expense.RejectionReason.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrow_WhenTitleIsEmpty(string? title)
    {
        var act = () => new Expense(Guid.NewGuid(), title!, null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        act.Should().Throw<ArgumentException>().WithMessage("*Title*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNull()
    {
        var act = () => new Expense(Guid.NewGuid(), "Test", null, null!, DateTime.UtcNow, Guid.NewGuid(), "user-1");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_ShouldThrow_WhenUserIdIsEmpty(string? userId)
    {
        var act = () => new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), userId!);
        act.Should().Throw<ArgumentException>().WithMessage("*User ID*");
    }

    // Submit
    [Fact]
    public void Submit_ShouldChangeStatusToSubmitted()
    {
        var expense = CreateDraftExpense();

        expense.Submit();

        expense.Status.Should().Be(ExpenseStatus.Submitted);
    }

    [Fact]
    public void Submit_ShouldThrow_WhenNotDraft()
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        var act = () => expense.Submit();

        act.Should().Throw<ExpenseDomainException>().WithMessage("*Submitted*");
    }

    // Approve
    [Fact]
    public void Approve_ShouldChangeStatusToApproved()
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        expense.Approve("manager-1");

        expense.Status.Should().Be(ExpenseStatus.Approved);
        expense.ApproverId.Should().Be("manager-1");
    }

    [Fact]
    public void Approve_ShouldThrow_WhenDraft()
    {
        var expense = CreateDraftExpense();

        var act = () => expense.Approve("manager-1");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*Draft*");
    }

    [Fact]
    public void Approve_ShouldThrow_WhenAlreadyRejected()
    {
        var expense = CreateDraftExpense();
        expense.Submit();
        expense.Reject("manager-1", "Too expensive");

        var act = () => expense.Approve("manager-2");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*Rejected*");
    }

    [Fact]
    public void Approve_ShouldThrow_WhenSelfApproving()
    {
        var expense = CreateDraftExpense("user-1");
        expense.Submit();

        var act = () => expense.Approve("user-1");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*owner*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Approve_ShouldThrow_WhenApproverIdIsEmpty(string? approverId)
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        var act = () => expense.Approve(approverId!);

        act.Should().Throw<ArgumentException>().WithMessage("*Approver*");
    }

    // Reject
    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        expense.Reject("manager-1", "Over budget");

        expense.Status.Should().Be(ExpenseStatus.Rejected);
        expense.ApproverId.Should().Be("manager-1");
        expense.RejectionReason.Should().Be("Over budget");
    }

    [Fact]
    public void Reject_ShouldThrow_WhenDraft()
    {
        var expense = CreateDraftExpense();

        var act = () => expense.Reject("manager-1", "Reason");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*Draft*");
    }

    [Fact]
    public void Reject_ShouldThrow_WhenAlreadyApproved()
    {
        var expense = CreateDraftExpense();
        expense.Submit();
        expense.Approve("manager-1");

        var act = () => expense.Reject("manager-2", "Changed mind");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*Approved*");
    }

    [Fact]
    public void Reject_ShouldThrow_WhenSelfRejecting()
    {
        var expense = CreateDraftExpense("user-1");
        expense.Submit();

        var act = () => expense.Reject("user-1", "Reason");

        act.Should().Throw<ExpenseDomainException>().WithMessage("*owner*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Reject_ShouldThrow_WhenReasonIsEmpty(string? reason)
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        var act = () => expense.Reject("manager-1", reason!);

        act.Should().Throw<ArgumentException>().WithMessage("*reason*");
    }

    // Update
    [Fact]
    public void Update_ShouldModifyDraftExpense()
    {
        var expense = CreateDraftExpense();
        var newAmount = new Money(100m, "EUR");
        var newCategoryId = Guid.NewGuid();

        expense.Update("Updated Title", "New desc", newAmount, DateTime.UtcNow, newCategoryId);

        expense.Title.Should().Be("Updated Title");
        expense.Amount.Should().Be(newAmount);
        expense.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void Update_ShouldThrow_WhenNotDraft()
    {
        var expense = CreateDraftExpense();
        expense.Submit();

        var act = () => expense.Update("New", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid());

        act.Should().Throw<ExpenseDomainException>().WithMessage("*draft*");
    }
}
