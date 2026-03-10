using ExpenseManager.Application.Common.Models;
using ExpenseManager.Application.Expenses.DTOs;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    string Title,
    string? Description,
    decimal Amount,
    string Currency,
    DateTime Date,
    Guid CategoryId) : IRequest<Result<ExpenseDto>>;
