using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
