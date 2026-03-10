using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _expenseRepository = expenseRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
            return Result.Failure($"Expense with ID '{request.Id}' was not found.");

        if (expense.UserId != _currentUserService.UserId)
            return Result.Failure("You can only delete your own expenses.");

        if (expense.Status != ExpenseStatus.Draft)
            return Result.Failure("Only draft expenses can be deleted.");

        _expenseRepository.Delete(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
