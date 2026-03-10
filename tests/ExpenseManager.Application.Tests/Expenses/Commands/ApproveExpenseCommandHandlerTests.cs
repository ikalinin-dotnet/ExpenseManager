using ExpenseManager.Application.Expenses.Commands.ApproveExpense;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ExpenseManager.Application.Tests.Expenses.Commands;

public class ApproveExpenseCommandHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ApproveExpenseCommandHandler _handler;

    public ApproveExpenseCommandHandlerTests()
    {
        _handler = new ApproveExpenseCommandHandler(_expenseRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldApproveExpense_WhenValid()
    {
        var expense = new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        expense.Submit();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _currentUserService.UserId.Returns("manager-1");

        var result = await _handler.Handle(new ApproveExpenseCommand(expense.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        expense.Status.Should().Be(ExpenseStatus.Approved);
        expense.ApproverId.Should().Be("manager-1");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSelfApproving()
    {
        var expense = new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        expense.Submit();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _currentUserService.UserId.Returns("user-1");

        var result = await _handler.Handle(new ApproveExpenseCommand(expense.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("owner");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotAuthenticated()
    {
        _currentUserService.UserId.Returns((string?)null);

        var result = await _handler.Handle(new ApproveExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not authenticated");
    }
}
