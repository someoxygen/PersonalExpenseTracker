using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;

public sealed class Budget : Entity
{
    private Budget() { }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }
    public Category Category { get; private set; } = null!;
    public static Budget Create(Guid owner, Category category, decimal amount, int month, int year)
    {
        var budget = new Budget { UserId = owner };
        budget.Update(category, amount, month, year);
        return budget;
    }
    public void Update(Category category, decimal amount, int month, int year)
    {
        if (category.UserId != UserId || category.Type != CategoryType.Expense) throw new DomainException("Budget requires your own expense category.");
        if (month is < 1 or > 12 || year is < 2000 or > 2100) throw new DomainException("Invalid budget period.");
        CategoryId = category.Id; Amount = Rules.Money(amount); Month = month; Year = year; Touch();
    }
}
public sealed record BudgetProgress(decimal BudgetAmount, decimal Spent)
{
    public decimal Remaining => BudgetAmount - Spent;
    public decimal Percentage => BudgetAmount > 0 ? decimal.Round(Spent / BudgetAmount * 100, 2) : 0;
}
