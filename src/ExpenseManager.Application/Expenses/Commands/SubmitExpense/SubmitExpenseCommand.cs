using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.SubmitExpense;

public sealed record SubmitExpenseCommand(Guid Id) : IRequest<Result>;
