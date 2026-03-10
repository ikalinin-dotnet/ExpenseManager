using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Expenses.Commands.ApproveExpense;

public sealed record ApproveExpenseCommand(Guid Id) : IRequest<Result>;
