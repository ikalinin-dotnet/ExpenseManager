using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using MediatR;

namespace ExpenseManager.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;
