using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Budgets;

public sealed class BudgetService(IAppDbContext db, ICurrentUser user, IFinancialClock clock, IValidator<BudgetRequest> validator)
{
    private IQueryable<BudgetResponse> Query(Guid? id, int? month, int? year) =>
        db.Budgets.AsNoTracking().Where(x => x.UserId == user.Id && (id == null || x.Id == id)
            && (month == null || x.Month == month) && (year == null || x.Year == year))
        .OrderBy(x => x.Category.Name)
        .Select(x => new BudgetResponse(x.Id, x.CategoryId, x.Category.Name, x.Amount,
            db.Transactions.Where(t => t.UserId == user.Id && t.CategoryId == x.CategoryId && t.Type == TransactionType.Expense
                && t.TransactionDate.Month == x.Month && t.TransactionDate.Year == x.Year && t.TransactionDate <= clock.Today).Sum(t => t.Amount), x.Month, x.Year));
    public Task<List<BudgetResponse>> List(int? month, int? year, CancellationToken ct)
    {
        var selectedMonth = month ?? clock.Today.Month; var selectedYear = year ?? clock.Today.Year;
        if (selectedMonth is < 1 or > 12 || selectedYear is < 2000 or > 2100) throw new AppException(400, "Invalid budget period.");
        return Query(null, selectedMonth, selectedYear).ToListAsync(ct);
    }
    public async Task<BudgetResponse> Get(Guid id, CancellationToken ct) => await Query(id, null, null).SingleOrDefaultAsync(ct) ?? throw AppException.NotFound();
    public async Task<BudgetResponse> Create(BudgetRequest request, CancellationToken ct)
    {
        var category = await Validate(request, ct);
        var budget = Budget.Create(user.Id, category, request.Amount, request.Month, request.Year);
        db.Budgets.Add(budget); await db.SaveChangesAsync(ct);
        return await Get(budget.Id, ct);
    }
    public async Task Update(Guid id, BudgetRequest request, CancellationToken ct)
    {
        var budget = await Owned(id, ct);
        budget.Update(await Validate(request, ct), request.Amount, request.Month, request.Year);
        await db.SaveChangesAsync(ct);
    }
    public async Task Delete(Guid id, CancellationToken ct)
    {
        db.Budgets.Remove(await Owned(id, ct)); await db.SaveChangesAsync(ct);
    }
    private async Task<Category> Validate(BudgetRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        return await db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == user.Id && x.Id == request.CategoryId, ct) ?? throw AppException.NotFound();
    }
    private async Task<Budget> Owned(Guid id, CancellationToken ct) => await db.Budgets.SingleOrDefaultAsync(x => x.UserId == user.Id && x.Id == id, ct) ?? throw AppException.NotFound();
}
