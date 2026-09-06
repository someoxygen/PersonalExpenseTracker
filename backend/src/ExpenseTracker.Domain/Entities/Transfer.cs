using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;
// One immutable record represents both legs; it can never contain a lone inflow/outflow.
public sealed class Transfer : Entity
{
    private Transfer() { }
    public Guid UserId { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public Guid TargetAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public string? Description { get; private set; }
    public Account SourceAccount { get; private set; } = null!;
    public Account TargetAccount { get; private set; } = null!;
    public static Transfer Create(Guid owner, Account source, Account target, decimal amount, DateOnly date, string? description)
    {
        if (source.Id == target.Id) throw new DomainException("Source and target must differ.");
        if (source.UserId != owner || target.UserId != owner) throw new DomainException("Both accounts must belong to you.");
        if (!source.IsActive || !target.IsActive) throw new DomainException("Both accounts must be active.");
        if (source.Currency != target.Currency) throw new DomainException("Cross-currency transfers are not supported.");
        if (date.Year is < 2000 or > 2100 || description?.Length > 500) throw new DomainException("Invalid transfer date or description.");
        return new Transfer
        {
            UserId = owner,
            SourceAccountId = source.Id,
            TargetAccountId = target.Id,
            Amount = Rules.Money(amount),
            TransactionDate = date,
            Description = description?.Trim()
        };
    }
}
