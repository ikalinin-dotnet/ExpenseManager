namespace ExpenseManager.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; internal set; }
    public DateTime UpdatedAtUtc { get; internal set; }

    protected AuditableEntity(Guid id) : base(id) { }

    protected AuditableEntity() { } // EF Core
}
