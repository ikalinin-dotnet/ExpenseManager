using ExpenseManager.Application.Common.Models;
using ExpenseManager.Domain.Interfaces;
using MediatR;

namespace ExpenseManager.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IExpenseRepository expenseRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _expenseRepository = expenseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure($"Category with ID '{request.Id}' was not found.");

        var expenses = await _expenseRepository.GetByCategoryIdAsync(request.Id, cancellationToken);
        if (expenses.Count > 0)
            return Result.Failure("Cannot delete a category that has associated expenses.");

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
