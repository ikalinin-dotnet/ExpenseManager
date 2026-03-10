using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid Id) : IRequest<Result>;
