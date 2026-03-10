using AutoMapper;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetExpensesByCategory;

public sealed class GetExpensesByCategoryQueryHandler
    : IRequestHandler<GetExpensesByCategoryQuery, Result<CategoryExpenseReportDto>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetExpensesByCategoryQueryHandler(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<CategoryExpenseReportDto>> Handle(
        GetExpensesByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryExpenseReportDto>($"Category with ID '{request.CategoryId}' was not found.");

        var expenses = await _expenseRepository.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

        var report = new CategoryExpenseReportDto
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            TotalAmount = expenses.Sum(e => e.Amount.Amount),
            ExpenseCount = expenses.Count,
            Expenses = _mapper.Map<IReadOnlyList<ExpenseByCategoryDto>>(expenses)
        };

        return Result.Success(report);
    }
}
