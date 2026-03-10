using AutoMapper;
using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>($"Category with ID '{request.Id}' was not found.");

        bool nameExists = await _categoryRepository.NameExistsAsync(request.Name, request.Id, cancellationToken);
        if (nameExists)
            return Result.Failure<CategoryDto>($"A category with the name '{request.Name}' already exists.");

        category.Update(request.Name, request.Description);

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }
}
