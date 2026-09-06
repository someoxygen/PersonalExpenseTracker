using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;

public enum TransactionType { Income, Expense }
public sealed class Transaction : Entity
{
    private Transaction() { }
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public Account Account { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public static Transaction Create(Guid owner, Account account, Category category, TransactionType type, decimal amount, string? description, DateOnly date)
    {
        var transaction = new Transaction { UserId = owner };
        transaction.Update(account, category, type, amount, description, date);
        return transaction;
    }
    public void Update(Account account, Category category, TransactionType type, decimal amount, string? description, DateOnly date)
    {
        if (account.UserId != UserId || category.UserId != UserId) throw new DomainException("Related records must belong to the current user.");
        if (!account.IsActive) throw new DomainException("Account is inactive.");
        if (!Enum.IsDefined(type) || (int)type != (int)category.Type) throw new DomainException("Transaction and category types must match.");
        if (date.Year is < 2000 or > 2100) throw new DomainException("Financial date must be between 2000 and 2100.");
        if (description?.Length > 500) throw new DomainException("Description is too long.");
        AccountId = account.Id; CategoryId = category.Id; Type = type;
        Amount = Rules.Money(amount); Description = description?.Trim(); TransactionDate = date;
        Touch();
    }
}
