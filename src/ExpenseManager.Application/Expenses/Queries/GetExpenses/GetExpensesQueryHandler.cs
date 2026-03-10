using AutoMapper;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Expenses.Queries.GetExpenses;

public sealed class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, Result<PaginatedList<ExpenseDto>>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IMapper _mapper;

    public GetExpensesQueryHandler(IExpenseRepository expenseRepository, IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _expenseRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.UserId,
            request.Status,
            request.CategoryId,
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ExpenseDto>>(items);
        var paginatedList = new PaginatedList<ExpenseDto>(dtos, totalCount, request.PageNumber, request.PageSize);

        return Result.Success(paginatedList);
    }
}
