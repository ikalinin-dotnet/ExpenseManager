using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? userId = null,
        ExpenseStatus? status = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountByUserAsync(string userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task AddAsync(Expense expense, CancellationToken cancellationToken = default);
    void Update(Expense expense);
    void Delete(Expense expense);
}
