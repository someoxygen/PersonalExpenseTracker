namespace ExpenseTracker.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
public sealed class DomainException(string message) : Exception(message);
public static class Rules
{
    public static string Text(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > max)
            throw new DomainException($"Text must contain 1 to {max} characters.");
        return value.Trim();
    }
    public static decimal Money(decimal value, bool positive = true)
    {
        if ((positive && value <= 0) || value < -9999999999999999.99m || value > 9999999999999999.99m || decimal.Round(value, 2) != value)
            throw new DomainException("Amount must fit 18 digits with at most 2 decimal places and be positive when required.");
        return value;
    }
    public static string Currency(string value)
    {
        var currency = Text(value, 3).ToUpperInvariant();
        if (currency is not ("TRY" or "USD" or "EUR")) throw new DomainException("Supported currencies: TRY, USD, EUR.");
        return currency;
    }
}
