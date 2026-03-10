using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Result<CategoryDto>>;
