using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Transactions;

public sealed class TransactionService(IAppDbContext db, ICurrentUser user, IValidator<TransactionRequest> validator, IValidator<TransactionQuery> queryValidator)
{
    public async Task<PageResponse<TransactionResponse>> List(TransactionQuery filter, CancellationToken ct)
    {
        await queryValidator.ValidateAndThrowAsync(filter, ct);
        var query = db.Transactions.AsNoTracking().Where(x => x.UserId == user.Id);
        if (filter.AccountId is { } account) query = query.Where(x => x.AccountId == account);
        if (filter.CategoryId is { } category) query = query.Where(x => x.CategoryId == category);
        if (filter.Type is { } type) query = query.Where(x => x.Type == type);
        if (filter.StartDate is { } start) query = query.Where(x => x.TransactionDate >= start);
        if (filter.EndDate is { } end) query = query.Where(x => x.TransactionDate <= end);
        if (filter.MinAmount is { } min) query = query.Where(x => x.Amount >= min);
        if (filter.MaxAmount is { } max) query = query.Where(x => x.Amount <= max);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Description != null && x.Description.ToLower().Contains(search));
        }
        var total = await query.CountAsync(ct);
        var ascending = filter.SortDirection == "asc";
        var sorted = filter.SortBy switch
        {
            "amount" => ascending ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
            "createdAt" => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
            _ => ascending ? query.OrderBy(x => x.TransactionDate) : query.OrderByDescending(x => x.TransactionDate)
        };
        var items = await sorted.ThenBy(x => x.Id).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .Select(TransactionResponse.Projection).ToListAsync(ct);
        return new(items, filter.Page, filter.PageSize, total, (int)Math.Ceiling(total / (double)filter.PageSize));
    }
    public async Task<TransactionResponse> Get(Guid id, CancellationToken ct) => await db.Transactions.AsNoTracking()
        .Where(x => x.UserId == user.Id && x.Id == id).Select(TransactionResponse.Projection).SingleOrDefaultAsync(ct) ?? throw AppException.NotFound();
    public async Task<TransactionResponse> Create(TransactionRequest request, CancellationToken ct)
    {
        var (account, category) = await Validate(request, ct);
        var transaction = Transaction.Create(user.Id, account, category, request.Type, request.Amount, request.Description, request.TransactionDate);
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);
        return await Get(transaction.Id, ct);
    }
    public async Task Update(Guid id, TransactionRequest request, CancellationToken ct)
    {
        var transaction = await Owned(id, ct);
        var (account, category) = await Validate(request, ct);
        transaction.Update(account, category, request.Type, request.Amount, request.Description, request.TransactionDate);
        await db.SaveChangesAsync(ct);
    }
    public async Task Delete(Guid id, CancellationToken ct)
    {
        db.Transactions.Remove(await Owned(id, ct));
        await db.SaveChangesAsync(ct);
    }
    private async Task<(Account, Category)> Validate(TransactionRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var account = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
        var category = await db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
        return (account, category);
    }
    private async Task<Transaction> Owned(Guid id, CancellationToken ct) => await db.Transactions.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
}
