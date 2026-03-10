using ExpenseManager.Domain.Interfaces;

namespace ExpenseManager.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
