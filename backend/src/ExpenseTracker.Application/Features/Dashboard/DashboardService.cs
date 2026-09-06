using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Features.Transactions;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Dashboard;

public sealed class DashboardService(IAppDbContext db, ICurrentUser user, IFinancialClock clock)
{
    public async Task<DashboardSummary> Summary(CancellationToken ct)
    {
        var today = clock.Today; var start = new DateOnly(today.Year, today.Month, 1);
        var currency = await db.Users.AsNoTracking().Where(x => x.Id == user.Id).Select(x => x.Currency).SingleAsync(ct);
        var initial = await db.Accounts.AsNoTracking().Where(x => x.UserId == user.Id).SumAsync(x => x.InitialBalance, ct);
        var values = await db.Transactions.AsNoTracking().Where(x => x.UserId == user.Id && x.TransactionDate <= today)
            .GroupBy(x => 1).Select(g => new
            {
                Balance = g.Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount),
                Income = g.Sum(x => x.TransactionDate >= start && x.Type == TransactionType.Income ? x.Amount : 0),
                Expense = g.Sum(x => x.TransactionDate >= start && x.Type == TransactionType.Expense ? x.Amount : 0)
            }).SingleOrDefaultAsync(ct);
        var budgets = db.Budgets.AsNoTracking().Where(x => x.UserId == user.Id && x.Month == today.Month && x.Year == today.Year);
        var budgetAmount = await budgets.SumAsync(x => x.Amount, ct);
        var spent = await db.Transactions.AsNoTracking().Where(x => x.UserId == user.Id && x.Type == TransactionType.Expense
            && x.TransactionDate >= start && x.TransactionDate <= today && budgets.Any(b => b.CategoryId == x.CategoryId)).SumAsync(x => x.Amount, ct);
        // All transfers are between this user's same-currency accounts, so their net total is zero.
        return new(initial + (values?.Balance ?? 0), values?.Income ?? 0, values?.Expense ?? 0,
            (values?.Income ?? 0) - (values?.Expense ?? 0), budgetAmount, spent, currency, today);
    }
    public async Task<List<MonthlyFinance>> Monthly(int months, CancellationToken ct)
    {
        if (months is not (6 or 12)) throw new AppException(400, "Months must be 6 or 12.");
        var today = clock.Today; var start = new DateOnly(today.Year, today.Month, 1).AddMonths(1 - months);
        var totals = await db.Transactions.AsNoTracking().Where(x => x.UserId == user.Id && x.TransactionDate >= start && x.TransactionDate <= today)
            .GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month })
            .Select(g => new MonthlyFinance(g.Key.Year, g.Key.Month,
                g.Sum(x => x.Type == TransactionType.Income ? x.Amount : 0),
                g.Sum(x => x.Type == TransactionType.Expense ? x.Amount : 0))).ToListAsync(ct);
        // At most 12 SQL aggregate rows; only fill missing calendar buckets here.
        return Enumerable.Range(0, months).Select(offset =>
        {
            var date = start.AddMonths(offset);
            return totals.SingleOrDefault(x => x.Year == date.Year && x.Month == date.Month) ?? new MonthlyFinance(date.Year, date.Month, 0, 0);
        }).ToList();
    }
    public async Task<List<CategoryExpense>> CategoryExpenses(CancellationToken ct)
    {
        var today = clock.Today; var start = new DateOnly(today.Year, today.Month, 1);
        var query = db.Transactions.AsNoTracking().Where(x => x.UserId == user.Id && x.Type == TransactionType.Expense
            && x.TransactionDate >= start && x.TransactionDate <= today);
        var total = await query.SumAsync(x => x.Amount, ct);
        return await query.GroupBy(x => new { x.CategoryId, x.Category.Name, x.Category.Color })
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Select(g => new CategoryExpense(g.Key.CategoryId, g.Key.Name, g.Key.Color, g.Sum(x => x.Amount),
                total > 0 ? Math.Round(g.Sum(x => x.Amount) / total * 100, 2) : 0)).ToListAsync(ct);
    }
    public Task<List<TransactionResponse>> Recent(CancellationToken ct) => db.Transactions.AsNoTracking()
        .Where(x => x.UserId == user.Id && x.TransactionDate <= clock.Today)
        .OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
        .Take(10).Select(TransactionResponse.Projection).ToListAsync(ct);
}
