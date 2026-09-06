using System.Linq.Expressions;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
namespace ExpenseTracker.Application.Features.Transactions;

public sealed record TransactionRequest(Guid AccountId, Guid CategoryId, TransactionType Type, decimal Amount, string? Description, DateOnly TransactionDate);
public sealed record TransactionResponse(Guid Id, Guid AccountId, string AccountName, Guid CategoryId, string CategoryName,
    TransactionType Type, decimal Amount, string? Description, DateOnly TransactionDate, DateTimeOffset CreatedAt)
{
    public static readonly Expression<Func<Transaction, TransactionResponse>> Projection = x =>
        new(x.Id, x.AccountId, x.Account.Name, x.CategoryId, x.Category.Name, x.Type, x.Amount, x.Description, x.TransactionDate, x.CreatedAt);
}
public sealed record PageResponse<T>(List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed class TransactionQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public TransactionType? Type { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "transactionDate";
    public string SortDirection { get; set; } = "desc";
}
public sealed class TransactionValidator : AbstractValidator<TransactionRequest>
{
    public TransactionValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty(); RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TransactionDate).InclusiveBetween(new DateOnly(2000, 1, 1), new DateOnly(2100, 12, 31));
    }
}
public sealed class TransactionQueryValidator : AbstractValidator<TransactionQuery>
{
    public TransactionQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.SortBy).Must(x => x is "transactionDate" or "amount" or "createdAt");
        RuleFor(x => x.SortDirection).Must(x => x is "asc" or "desc");
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount).WithMessage("Invalid amount range.");
        RuleFor(x => x).Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate).WithMessage("Invalid date range.");
    }
}
