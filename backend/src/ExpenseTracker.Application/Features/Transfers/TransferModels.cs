using System.Linq.Expressions;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
namespace ExpenseTracker.Application.Features.Transfers;

public sealed record TransferRequest(Guid SourceAccountId, Guid TargetAccountId, decimal Amount, DateOnly TransactionDate, string? Description);
public sealed record TransferResponse(Guid Id, Guid SourceAccountId, string SourceAccountName, Guid TargetAccountId,
    string TargetAccountName, decimal Amount, DateOnly TransactionDate, string? Description)
{
    public static readonly Expression<Func<Transfer, TransferResponse>> Projection = x =>
        new(x.Id, x.SourceAccountId, x.SourceAccount.Name, x.TargetAccountId, x.TargetAccount.Name, x.Amount, x.TransactionDate, x.Description);
}
public sealed class TransferValidator : AbstractValidator<TransferRequest>
{
    public TransferValidator()
    {
        RuleFor(x => x.SourceAccountId).NotEmpty().NotEqual(x => x.TargetAccountId);
        RuleFor(x => x.TargetAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TransactionDate).InclusiveBetween(new DateOnly(2000, 1, 1), new DateOnly(2100, 12, 31));
    }
}
