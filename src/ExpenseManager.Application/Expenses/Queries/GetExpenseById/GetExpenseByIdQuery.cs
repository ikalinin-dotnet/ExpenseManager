using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using MediatR;

namespace ExpenseManager.Application.Expenses.Queries.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid Id) : IRequest<Result<ExpenseDto>>;
