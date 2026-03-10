namespace ExpenseManager.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    protected AuditableEntity(Guid id) : base(id) { }

    protected AuditableEntity() { } // EF Core
}
