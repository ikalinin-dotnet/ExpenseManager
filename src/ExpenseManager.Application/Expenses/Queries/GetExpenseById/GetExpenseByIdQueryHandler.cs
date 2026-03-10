using AutoMapper;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseDto>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IMapper _mapper;

    public GetExpenseByIdQueryHandler(IExpenseRepository expenseRepository, IMapper mapper)
    {
        _expenseRepository = expenseRepository;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
            return Result.Failure<ExpenseDto>($"Expense with ID '{request.Id}' was not found.");

        return Result.Success(_mapper.Map<ExpenseDto>(expense));
    }
}
