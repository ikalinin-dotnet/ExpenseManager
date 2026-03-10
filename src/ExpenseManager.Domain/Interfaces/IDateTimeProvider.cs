namespace ExpenseManager.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
