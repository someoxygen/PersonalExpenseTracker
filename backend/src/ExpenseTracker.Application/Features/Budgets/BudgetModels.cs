using ExpenseTracker.Domain.Entities;
using FluentValidation;
namespace ExpenseTracker.Application.Features.Budgets;

public sealed record BudgetRequest(Guid CategoryId, decimal Amount, int Month, int Year);
public sealed record BudgetResponse(Guid Id, Guid CategoryId, string CategoryName, decimal BudgetAmount, decimal Spent, int Month, int Year)
{
    public decimal Remaining => new BudgetProgress(BudgetAmount, Spent).Remaining;
    public decimal Percentage => new BudgetProgress(BudgetAmount, Spent).Percentage;
}
public sealed class BudgetValidator : AbstractValidator<BudgetRequest>
{
    public BudgetValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
