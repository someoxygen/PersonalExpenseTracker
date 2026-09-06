using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Application.Features.Categories;

public sealed class CategoryService(IAppDbContext db, ICurrentUser user, IValidator<CategoryRequest> validator)
{
    public Task<List<CategoryResponse>> List(CancellationToken ct) => db.Categories.AsNoTracking()
        .Where(x => x.UserId == user.Id).OrderBy(x => x.Type).ThenBy(x => x.Name).Select(CategoryResponse.Projection).ToListAsync(ct);
    public async Task<CategoryResponse> Get(Guid id, CancellationToken ct) => await db.Categories.AsNoTracking()
        .Where(x => x.UserId == user.Id && x.Id == id).Select(CategoryResponse.Projection).SingleOrDefaultAsync(ct) ?? throw AppException.NotFound();
    public async Task<CategoryResponse> Create(CategoryRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var category = Category.Create(user.Id, request.Name, request.Type, request.Icon, request.Color);
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return await Get(category.Id, ct);
    }
    public async Task Update(Guid id, CategoryRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var category = await Owned(id, ct);
        category.Update(request.Name, request.Type, request.Icon, request.Color);
        await db.SaveChangesAsync(ct);
    }
    public async Task Delete(Guid id, CancellationToken ct)
    {
        var category = await Owned(id, ct);
        category.EnsureEditable();
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }
    private async Task<Category> Owned(Guid id, CancellationToken ct) => await db.Categories.SingleOrDefaultAsync(x => x.Id == id && x.UserId == user.Id, ct)
        ?? throw AppException.NotFound();
}
