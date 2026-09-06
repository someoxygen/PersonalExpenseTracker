using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Accounts;

public sealed class AccountService(IAppDbContext db, ICurrentUser user, IValidator<AccountRequest> validator, IFinancialClock clock)
{
    private IQueryable<AccountResponse> Query(Guid? id = null) => db.Accounts.AsNoTracking().Where(x => x.UserId == user.Id && (id == null || x.Id == id)).OrderBy(x => x.Name)
        .Select(x => new AccountResponse(x.Id, x.Name, x.Type, x.InitialBalance,
            x.InitialBalance + db.Transactions.Where(t => t.UserId == user.Id && t.AccountId == x.Id && t.TransactionDate <= clock.Today)
                .Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount)
            + db.Transfers.Where(t => t.UserId == user.Id && t.TargetAccountId == x.Id && t.TransactionDate <= clock.Today).Sum(t => t.Amount)
            - db.Transfers.Where(t => t.UserId == user.Id && t.SourceAccountId == x.Id && t.TransactionDate <= clock.Today).Sum(t => t.Amount), x.Currency, x.IsActive));
    public Task<List<AccountResponse>> List(CancellationToken ct) => Query().ToListAsync(ct);
    public async Task<AccountResponse> Get(Guid id, CancellationToken ct) => await Query(id).SingleOrDefaultAsync(ct) ?? throw AppException.NotFound();
    public async Task<AccountResponse> Create(AccountRequest request, CancellationToken ct)
    {
        await Validate(request, ct);
        var account = Account.Create(user.Id, request.Name, request.Type, request.InitialBalance, request.Currency);
        if (!request.IsActive) account.Deactivate();
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return await Get(account.Id, ct);
    }
    public async Task Update(Guid id, AccountRequest request, CancellationToken ct)
    {
        await Validate(request, ct);
        var account = await Owned(id, ct);
        if (request.Currency != account.Currency) throw new AppException(400, "Account currency cannot be changed.");
        account.Update(request.Name, request.Type, request.InitialBalance, request.IsActive);
        await db.SaveChangesAsync(ct);
    }
    public async Task Delete(Guid id, CancellationToken ct)
    {
        (await Owned(id, ct)).Deactivate();
        await db.SaveChangesAsync(ct);
    }
    private async Task Validate(AccountRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var currency = await db.Users.Where(x => x.Id == user.Id).Select(x => x.Currency).SingleAsync(ct);
        if (request.Currency != currency) throw new AppException(400, "Accounts must use your profile currency. Currency conversion is not supported.");
    }
    private async Task<Account> Owned(Guid id, CancellationToken ct) => await db.Accounts.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
}
