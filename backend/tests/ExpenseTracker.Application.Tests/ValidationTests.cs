using ExpenseTracker.Application.Features.Transactions;
using ExpenseTracker.Application.Features.Budgets;
using ExpenseTracker.Domain.Entities;
namespace ExpenseTracker.Application.Tests;

public sealed class ValidationTests
{
    [Fact]
    public async Task Transaction_validation_protects_ids_dates_and_precision()
    {
        var validator = new TransactionValidator();
        var valid = new TransactionRequest(Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense, 12.34m, null, new(2026, 1, 1));
        Assert.True((await validator.ValidateAsync(valid)).IsValid);
        foreach (var invalid in new[] { valid with { AccountId = Guid.Empty }, valid with { CategoryId = Guid.Empty },
            valid with { Amount = 0 }, valid with { Amount = 0.001m }, valid with { Type = (TransactionType)22 },
            valid with { TransactionDate = default }, valid with { Description = new string('a', 501) } })
            Assert.False((await validator.ValidateAsync(invalid)).IsValid);
    }
    [Theory]
    [InlineData("amount", true)]
    [InlineData("transactionDate", true)]
    [InlineData("createdAt", true)]
    [InlineData("UserId", false)]
    [InlineData("amount;DROP TABLE", false)]
    public async Task Sort_fields_are_whitelisted(string field, bool valid) =>
        Assert.Equal(valid, (await new TransactionQueryValidator().ValidateAsync(new TransactionQuery { SortBy = field })).IsValid);
    [Fact]
    public async Task Budget_validation_rejects_invalid_period_and_amount()
    {
        var validator = new BudgetValidator();
        Assert.False((await validator.ValidateAsync(new BudgetRequest(Guid.NewGuid(), 0, 13, 1999))).IsValid);
        Assert.True((await validator.ValidateAsync(new BudgetRequest(Guid.NewGuid(), 100, 1, 2026))).IsValid);
    }
}
