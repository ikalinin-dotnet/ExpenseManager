using ExpenseManager.Domain.Common;
using ExpenseManager.Domain.Enums;
using ExpenseManager.Domain.Exceptions;
using ExpenseManager.Domain.ValueObjects;

namespace ExpenseManager.Domain.Entities;

public sealed class Expense : AuditableEntity
{
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTime Date { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string UserId { get; private set; } = null!;
    public string? ApproverId { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    private Expense() { } // EF Core

    public Expense(
        Guid id,
        string title,
        string? description,
        Money amount,
        DateTime date,
        Guid categoryId,
        string userId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));

        Title = title;
        Description = description;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Date = date;
        CategoryId = categoryId;
        UserId = userId;
        Status = ExpenseStatus.Draft;
    }

    public void Update(string title, string? description, Money amount, DateTime date, Guid categoryId)
    {
        if (Status != ExpenseStatus.Draft)
            throw new ExpenseDomainException("Only draft expenses can be edited.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title;
        Description = description;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Date = date;
        CategoryId = categoryId;
    }

    public void Submit()
    {
        if (Status != ExpenseStatus.Draft)
            throw new ExpenseDomainException($"Cannot submit an expense with status '{Status}'. Only draft expenses can be submitted.");

        Status = ExpenseStatus.Submitted;
    }

    public void Approve(string approverId)
    {
        if (string.IsNullOrWhiteSpace(approverId))
            throw new ArgumentException("Approver ID is required.", nameof(approverId));

        if (Status != ExpenseStatus.Submitted)
            throw new ExpenseDomainException($"Cannot approve an expense with status '{Status}'. Only submitted expenses can be approved.");

        if (approverId == UserId)
            throw new ExpenseDomainException("An expense cannot be approved by its owner.");

        ApproverId = approverId;
        Status = ExpenseStatus.Approved;
    }

    public void Reject(string approverId, string reason)
    {
        if (string.IsNullOrWhiteSpace(approverId))
            throw new ArgumentException("Approver ID is required.", nameof(approverId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        if (Status != ExpenseStatus.Submitted)
            throw new ExpenseDomainException($"Cannot reject an expense with status '{Status}'. Only submitted expenses can be rejected.");

        if (approverId == UserId)
            throw new ExpenseDomainException("An expense cannot be rejected by its owner.");

        ApproverId = approverId;
        RejectionReason = reason;
        Status = ExpenseStatus.Rejected;
    }
}
