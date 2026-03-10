using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Exceptions;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.RejectExpense;

public sealed class RejectExpenseCommandHandler : IRequestHandler<RejectExpenseCommand, Result>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectExpenseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Result.Failure("User is not authenticated.");

        var expense = await _expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
            return Result.Failure($"Expense with ID '{request.Id}' was not found.");

        try
        {
            expense.Reject(_currentUserService.UserId, request.Reason);
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
