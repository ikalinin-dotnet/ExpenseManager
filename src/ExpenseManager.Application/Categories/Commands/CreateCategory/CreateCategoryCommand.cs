using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description) : IRequest<Result<CategoryDto>>;
