using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;
