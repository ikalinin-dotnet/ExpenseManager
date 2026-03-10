using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Reports.DTOs;
using MediatR;

namespace ExpenseManager.Application.Reports.Queries.GetExpensesByCategory;

public sealed record GetExpensesByCategoryQuery(Guid CategoryId) : IRequest<Result<CategoryExpenseReportDto>>;
