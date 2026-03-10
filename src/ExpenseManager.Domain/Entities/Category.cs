using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.Entities;

public sealed class Category : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private readonly List<Expense> _expenses = new();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private Category() { } // EF Core

    public Category(Guid id, string name, string? description = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name;
        Description = description;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        Name = name;
        Description = description;
    }
}
