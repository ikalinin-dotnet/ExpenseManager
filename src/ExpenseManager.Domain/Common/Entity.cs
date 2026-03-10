namespace ExpenseManager.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { } // EF Core

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
