using AutoMapper;
using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
        return Result.Success(dtos);
    }
}
