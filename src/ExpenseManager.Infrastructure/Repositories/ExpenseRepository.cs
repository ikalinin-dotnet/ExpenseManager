using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Interfaces;
using ExpenseManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Infrastructure.Repositories;

public sealed class ExpenseRepository : RepositoryBase<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? userId = null,
        ExpenseStatus? status = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(e => e.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(e => e.UserId == userId);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        int totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Expense>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Expense>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(e => e.Category)
            .Where(e => e.CategoryId == categoryId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalAmountByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(e => e.UserId == userId);

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        // EF Core can't translate owned type navigation in Sum, so materialize first
        var expenses = await query.ToListAsync(cancellationToken);
        return expenses.Sum(e => e.Amount.Amount);
    }
}
