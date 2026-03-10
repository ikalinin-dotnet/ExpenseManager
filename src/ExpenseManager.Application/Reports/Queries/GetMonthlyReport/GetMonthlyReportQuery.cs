using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetMonthlyReport;

public sealed record GetMonthlyReportQuery(int Year, int Month, string? UserId = null) : IRequest<Result<MonthlyReportDto>>;
