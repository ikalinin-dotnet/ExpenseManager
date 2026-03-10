using AutoMapper;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Domain.Exceptions;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Domain.ValueObjects;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, Result<ExpenseDto>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateExpenseCommandHandler(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
            return Result.Failure<ExpenseDto>($"Expense with ID '{request.Id}' was not found.");

        if (expense.UserId != _currentUserService.UserId)
            return Result.Failure<ExpenseDto>("You can only edit your own expenses.");

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure<ExpenseDto>($"Category with ID '{request.CategoryId}' was not found.");

        try
        {
            var money = new Money(request.Amount, request.Currency);
            expense.Update(request.Title, request.Description, money, request.Date, request.CategoryId);
        }
        catch (ExpenseDomainException ex)
        {
            return Result.Failure<ExpenseDto>(ex.Message);
        }

        _expenseRepository.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _expenseRepository.GetByIdAsync(expense.Id, cancellationToken);
        return Result.Success(_mapper.Map<ExpenseDto>(updated));
    }
}
