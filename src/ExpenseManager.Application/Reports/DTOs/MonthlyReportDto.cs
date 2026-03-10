namespace ExpenseManager.Application.Reports.DTOs;

public sealed class MonthlyReportDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalAmount { get; init; }
    public int ExpenseCount { get; init; }
    public IReadOnlyList<MonthlyReportItemDto> Items { get; init; } = Array.Empty<MonthlyReportItemDto>();
}

public sealed class MonthlyReportItemDto
{
    public string CategoryName { get; init; } = null!;
    public decimal Amount { get; init; }
    public int Count { get; init; }
}
