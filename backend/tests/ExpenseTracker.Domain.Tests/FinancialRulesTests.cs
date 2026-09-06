using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Entities;
namespace ExpenseTracker.Domain.Tests;

public sealed class FinancialRulesTests
{
    [Theory]
    [InlineData(1000, 250, 750, 25)]
    [InlineData(200, 250, -50, 125)]
    [InlineData(0, 0, 0, 0)]
    public void Budget_progress_handles_remaining_overspending_and_zero(int budget, int spent, int remaining, int percentage)
    {
        var result = new BudgetProgress(budget, spent);
        Assert.Equal(remaining, result.Remaining); Assert.Equal(percentage, result.Percentage);
    }
    [Fact]
    public void Money_rejects_scale_and_overflow_without_rounding()
    {
        Assert.Throws<DomainException>(() => Rules.Money(0));
        Assert.Throws<DomainException>(() => Rules.Money(-1));
        Assert.Throws<DomainException>(() => Rules.Money(1.001m));
        Assert.Throws<DomainException>(() => Rules.Money(decimal.MinValue, false));
        Assert.Equal(-200, Rules.Money(-200, false));
        Assert.Equal(1.23m, Rules.Money(1.2300m));
    }
    [Fact]
    public void Transaction_requires_active_owned_account_and_matching_category()
    {
        var owner = Guid.NewGuid();
        var account = Account.Create(owner, "Cash", AccountType.Cash, 0, "TRY");
        var category = Category.Create(owner, "Food", CategoryType.Expense, "tag", "#123456");
        Assert.Throws<DomainException>(() => Transaction.Create(owner, account, category, TransactionType.Income, 1, null, new(2026, 1, 1)));
        Assert.Throws<DomainException>(() => Transaction.Create(Guid.NewGuid(), account, category, TransactionType.Expense, 1, null, new(2026, 1, 1)));
        account.Deactivate();
        Assert.Throws<DomainException>(() => Transaction.Create(owner, account, category, TransactionType.Expense, 1, null, new(2026, 1, 1)));
    }
    [Fact]
    public void Transfer_rejects_cross_currency_and_retains_both_accounts()
    {
        var owner = Guid.NewGuid(); var a = Account.Create(owner, "A", AccountType.Bank, 0, "TRY");
        var b = Account.Create(owner, "B", AccountType.Cash, 0, "TRY");
        var foreignCurrency = Account.Create(owner, "USD", AccountType.Bank, 0, "USD");
        Assert.Throws<DomainException>(() => Transfer.Create(owner, a, foreignCurrency, 10, new(2026, 1, 1), null));
        var transfer = Transfer.Create(owner, a, b, 10, new(2026, 1, 1), null);
        Assert.Equal(a.Id, transfer.SourceAccountId); Assert.Equal(b.Id, transfer.TargetAccountId);
    }
}
