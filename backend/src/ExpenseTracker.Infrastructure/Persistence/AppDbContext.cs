using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct) =>
        new AppTransaction(await Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct));
    private sealed class AppTransaction(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : IAppTransaction
    {
        public async Task CommitAsync(CancellationToken ct)
        {
            try { await transaction.CommitAsync(ct); }
            catch (PostgresException e) when (e.SqlState == "40001")
            { throw AppException.Conflict("Concurrent change detected. Retry the operation."); }
        }
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { return await base.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "40001" })
        { throw AppException.Conflict("Concurrent change detected. Retry the operation."); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" })
        { throw AppException.Conflict("A record with the same unique fields already exists."); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23503" })
        { throw AppException.Conflict("This record is referenced or a related record no longer exists."); }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
