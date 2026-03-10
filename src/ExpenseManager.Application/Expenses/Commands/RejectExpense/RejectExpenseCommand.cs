using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.RejectExpense;

public sealed record RejectExpenseCommand(Guid Id, string Reason) : IRequest<Result>;
