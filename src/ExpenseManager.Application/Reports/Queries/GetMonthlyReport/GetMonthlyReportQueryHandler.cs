using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetMonthlyReport;

public sealed class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, Result<MonthlyReportDto>>
{
    private readonly IExpenseRepository _expenseRepository;

    public GetMonthlyReportQueryHandler(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Result<MonthlyReportDto>> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var expenses = request.UserId is not null
            ? await _expenseRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            : await _expenseRepository.GetAllAsync(cancellationToken);

        var monthlyExpenses = expenses
            .Where(e => e.Date.Year == request.Year && e.Date.Month == request.Month)
            .ToList();

        var items = monthlyExpenses
            .GroupBy(e => e.Category?.Name ?? "Uncategorized")
            .Select(g => new MonthlyReportItemDto
            {
                CategoryName = g.Key,
                Amount = g.Sum(e => e.Amount.Amount),
                Count = g.Count()
            })
            .OrderByDescending(i => i.Amount)
            .ToList();

        var report = new MonthlyReportDto
        {
            Year = request.Year,
            Month = request.Month,
            TotalAmount = monthlyExpenses.Sum(e => e.Amount.Amount),
            ExpenseCount = monthlyExpenses.Count,
            Items = items
        };

        return Result.Success(report);
    }
}
