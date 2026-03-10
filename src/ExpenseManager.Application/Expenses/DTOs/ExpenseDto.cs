namespace ExpenseManager.Application.Expenses.DTOs;

public sealed class ExpenseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateTime Date { get; init; }
    public Guid CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string UserId { get; init; } = null!;
    public string? ApproverId { get; init; }
    public string Status { get; init; } = null!;
    public string? RejectionReason { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
