using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;

public enum AccountType { Cash, Bank, CreditCard, Savings, Other }
public sealed class Account : Entity
{
    private Account() { }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = "";
    public AccountType Type { get; private set; }
    public decimal InitialBalance { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public bool IsActive { get; private set; } = true;
    public static Account Create(Guid userId, string name, AccountType type, decimal balance, string currency)
    {
        var account = new Account { UserId = userId, Currency = Rules.Currency(currency) };
        account.Update(name, type, balance, true);
        return account;
    }
    public void Update(string name, AccountType type, decimal balance, bool active)
    {
        if (!Enum.IsDefined(type)) throw new DomainException("Invalid account type.");
        Name = Rules.Text(name, 100);
        Type = type;
        InitialBalance = Rules.Money(balance, false);
        IsActive = active;
        Touch();
    }
    public void Deactivate() { IsActive = false; Touch(); }
}
