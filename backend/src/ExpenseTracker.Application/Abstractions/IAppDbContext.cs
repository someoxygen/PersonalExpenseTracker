using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Abstractions;
// EF's provider-independent query API keeps filtering and aggregation in SQL.
// The concrete context, provider and transaction implementation live in Infrastructure.
public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Transfer> Transfers { get; }
    DbSet<Budget> Budgets { get; }
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ICurrentUser { Guid Id { get; } }
public interface IFinancialClock { DateOnly Today { get; } }
public interface IAppTransaction : IAsyncDisposable { Task CommitAsync(CancellationToken ct); }
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string? hash);
}
public interface ITokenService { (string Token, DateTimeOffset ExpiresAt) Create(Guid userId); }
