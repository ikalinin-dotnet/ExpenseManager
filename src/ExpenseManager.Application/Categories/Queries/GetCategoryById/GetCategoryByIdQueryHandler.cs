using AutoMapper;
using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>($"Category with ID '{request.Id}' was not found.");

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }
}
