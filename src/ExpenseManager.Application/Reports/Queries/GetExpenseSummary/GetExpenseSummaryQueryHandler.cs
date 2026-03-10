using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetExpenseSummary;

public sealed class GetExpenseSummaryQueryHandler : IRequestHandler<GetExpenseSummaryQuery, Result<ExpenseSummaryDto>>
{
    private readonly IExpenseRepository _expenseRepository;

    public GetExpenseSummaryQueryHandler(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<ExpenseSummaryDto>> Handle(GetExpenseSummaryQuery request, CancellationToken cancellationToken)
    {
        var expenses = request.UserId is not null
            ? await _expenseRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            : await _expenseRepository.GetAllAsync(cancellationToken);

        var summary = new ExpenseSummaryDto
        {
            TotalAmount = expenses.Sum(e => e.Amount.Amount),
            TotalCount = expenses.Count,
            DraftCount = expenses.Count(e => e.Status == ExpenseStatus.Draft),
            SubmittedCount = expenses.Count(e => e.Status == ExpenseStatus.Submitted),
            ApprovedCount = expenses.Count(e => e.Status == ExpenseStatus.Approved),
            RejectedCount = expenses.Count(e => e.Status == ExpenseStatus.Rejected),
            ApprovedAmount = expenses.Where(e => e.Status == ExpenseStatus.Approved).Sum(e => e.Amount.Amount)
        };

        return Result.Success(summary);
    }
}
