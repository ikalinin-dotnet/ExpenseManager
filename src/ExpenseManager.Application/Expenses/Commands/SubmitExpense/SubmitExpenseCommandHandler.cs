using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Exceptions;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.SubmitExpense;

public sealed class SubmitExpenseCommandHandler : IRequestHandler<SubmitExpenseCommand, Result>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
            return Result.Failure($"Expense with ID '{request.Id}' was not found.");

        if (expense.UserId != _currentUserService.UserId)
            return Result.Failure("You can only submit your own expenses.");

        try
        {
            expense.Submit();
        }
        catch (ExpenseDomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _expenseRepository.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
