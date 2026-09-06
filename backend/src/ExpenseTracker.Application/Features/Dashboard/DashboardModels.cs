namespace ExpenseTracker.Application.Features.Dashboard;

public sealed record DashboardSummary(decimal CurrentBalance, decimal ThisMonthIncome, decimal ThisMonthExpense,
    decimal NetBalance, decimal BudgetAmount, decimal BudgetSpent, string Currency, DateOnly FinancialDate)
{
    public decimal BudgetPercentage => BudgetAmount > 0 ? decimal.Round(BudgetSpent / BudgetAmount * 100, 2) : 0;
}
public sealed record MonthlyFinance(int Year, int Month, decimal Income, decimal Expense);
public sealed record CategoryExpense(Guid CategoryId, string CategoryName, string Color, decimal Amount, decimal Percentage);
