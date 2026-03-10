using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Domain.Enums;
using MediatR;

namespace ExpenseManager.Application.Expenses.Queries.GetExpenses;

public sealed record GetExpensesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? UserId = null,
    ExpenseStatus? Status = null,
    Guid? CategoryId = null) : IRequest<Result<PaginatedList<ExpenseDto>>>;
