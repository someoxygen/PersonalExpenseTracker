using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Features.Transactions;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Transfers;

public sealed class TransferService(IAppDbContext db, ICurrentUser user, IValidator<TransferRequest> validator)
{
    public async Task<TransferResponse> Create(TransferRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        await using var transaction = await db.BeginTransactionAsync(ct);
        var source = await db.Accounts.SingleOrDefaultAsync(x => x.Id == request.SourceAccountId && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
        var target = await db.Accounts.SingleOrDefaultAsync(x => x.Id == request.TargetAccountId && x.UserId == user.Id, ct) ?? throw AppException.NotFound();
        var transfer = Transfer.Create(user.Id, source, target, request.Amount, request.TransactionDate, request.Description);
        db.Transfers.Add(transfer);
        await db.SaveChangesAsync(ct);
        var result = await Get(transfer.Id, ct);
        await transaction.CommitAsync(ct);
        return result;
    }
    public async Task<TransferResponse> Get(Guid id, CancellationToken ct) => await db.Transfers.AsNoTracking()
        .Where(x => x.UserId == user.Id && x.Id == id).Select(TransferResponse.Projection).SingleOrDefaultAsync(ct) ?? throw AppException.NotFound();
    public async Task<PageResponse<TransferResponse>> List(int page, int pageSize, CancellationToken ct)
    {
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100) throw new AppException(400, "Invalid pagination.");
        var query = db.Transfers.AsNoTracking().Where(x => x.UserId == user.Id);
        var count = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.TransactionDate).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).Select(TransferResponse.Projection).ToListAsync(ct);
        return new(items, page, pageSize, count, (int)Math.Ceiling(count / (double)pageSize));
    }
}
