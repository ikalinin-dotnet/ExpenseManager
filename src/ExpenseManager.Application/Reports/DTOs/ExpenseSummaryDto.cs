namespace ExpenseManager.Application.Reports.DTOs;

public sealed class ExpenseSummaryDto
{
    public decimal TotalAmount { get; init; }
    public int TotalCount { get; init; }
    public int DraftCount { get; init; }
    public int SubmittedCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public decimal ApprovedAmount { get; init; }
}
