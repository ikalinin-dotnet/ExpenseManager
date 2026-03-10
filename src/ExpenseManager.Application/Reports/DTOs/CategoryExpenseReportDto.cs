namespace ExpenseManager.Application.Reports.DTOs;

public sealed class CategoryExpenseReportDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public int ExpenseCount { get; init; }
    public IReadOnlyList<ExpenseByCategoryDto> Expenses { get; init; } = Array.Empty<ExpenseByCategoryDto>();
}

public sealed class ExpenseByCategoryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTime Date { get; init; }
    public string Status { get; init; } = null!;
}
