using AutoMapper;
using ExpenseManager.Application.Common.Mappings;
using ExpenseManager.Application.Expenses.Commands.CreateExpense;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ExpenseManager.Application.Tests.Expenses.Commands;

public class CreateExpenseCommandHandlerTests
{
    private readonly IExpenseRepository _expenseRepository = Substitute.For<IExpenseRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly CreateExpenseCommandHandler _handler;

    public CreateExpenseCommandHandlerTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _handler = new CreateExpenseCommandHandler(
            _expenseRepository, _categoryRepository, _currentUserService, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        _currentUserService.UserId.Returns((string?)null);
        var command = new CreateExpenseCommand("Test", null, 100m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not authenticated");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        _currentUserService.UserId.Returns("user-1");
        _categoryRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateExpenseCommand("Test", null, 100m, "USD", DateTime.UtcNow, Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldCreateExpense_WhenValid()
    {
        var categoryId = Guid.NewGuid();
        _currentUserService.UserId.Returns("user-1");
        _categoryRepository.ExistsAsync(categoryId, Arg.Any<CancellationToken>()).Returns(true);
        _expenseRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.ArgAt<Guid>(0);
                return new Expense(id, "Test", null, new Domain.ValueObjects.Money(100m), DateTime.UtcNow, categoryId, "user-1");
            });

        var command = new CreateExpenseCommand("Test", null, 100m, "USD", DateTime.UtcNow, categoryId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test");
        await _expenseRepository.Received(1).AddAsync(Arg.Any<Expense>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
