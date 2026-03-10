namespace ExpenseManager.Domain.Exceptions;

public sealed class ExpenseDomainException : Exception
{
    public ExpenseDomainException(string message) : base(message) { }

    public ExpenseDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
