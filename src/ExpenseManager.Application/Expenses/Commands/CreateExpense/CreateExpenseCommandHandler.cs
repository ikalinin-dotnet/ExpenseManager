using AutoMapper;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Domain.ValueObjects;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<ExpenseDto>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateExpenseCommandHandler(
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

    public async Task<Result<ExpenseDto>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is null)
            return Result.Failure<ExpenseDto>("User is not authenticated.");

        bool categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure<ExpenseDto>($"Category with ID '{request.CategoryId}' was not found.");

        var money = new Money(request.Amount, request.Currency);

        var expense = new Expense(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            money,
            request.Date,
            request.CategoryId,
            _currentUserService.UserId);

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdExpense = await _expenseRepository.GetByIdAsync(expense.Id, cancellationToken);
        return Result.Success(_mapper.Map<ExpenseDto>(createdExpense));
    }
}
