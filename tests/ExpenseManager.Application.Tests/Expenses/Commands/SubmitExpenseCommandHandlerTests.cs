using ExpenseManager.Application.Expenses.Commands.SubmitExpense;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace ExpenseManager.Application.Tests.Expenses.Commands;

public class SubmitExpenseCommandHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SubmitExpenseCommandHandler _handler;

    public SubmitExpenseCommandHandlerTests()
    {
        _handler = new SubmitExpenseCommandHandler(_expenseRepository, _currentUserService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExpenseNotFound()
    {
        _expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Expense?)null);

        var result = await _handler.Handle(new SubmitExpenseCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotOwner()
    {
        var expense = new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _currentUserService.UserId.Returns("user-2");

        var result = await _handler.Handle(new SubmitExpenseCommand(expense.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("own expenses");
    }

    [Fact]
    public async Task Handle_ShouldSubmitExpense_WhenValid()
    {
        var expense = new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _currentUserService.UserId.Returns("user-1");

        var result = await _handler.Handle(new SubmitExpenseCommand(expense.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        expense.Status.Should().Be(ExpenseStatus.Submitted);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExpenseAlreadySubmitted()
    {
        var expense = new Expense(Guid.NewGuid(), "Test", null, new Money(10m), DateTime.UtcNow, Guid.NewGuid(), "user-1");
        expense.Submit();
        _expenseRepository.GetByIdAsync(expense.Id, Arg.Any<CancellationToken>()).Returns(expense);
        _currentUserService.UserId.Returns("user-1");

        var result = await _handler.Handle(new SubmitExpenseCommand(expense.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Submitted");
    }
}
