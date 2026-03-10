using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetExpenseSummary;

public sealed record GetExpenseSummaryQuery(string? UserId = null) : IRequest<Result<ExpenseSummaryDto>>;
