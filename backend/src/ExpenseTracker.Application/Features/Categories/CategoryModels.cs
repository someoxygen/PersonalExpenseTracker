using System.Linq.Expressions;
using ExpenseTracker.Domain.Entities;
using FluentValidation;
namespace ExpenseTracker.Application.Features.Categories;

public sealed record CategoryRequest(string Name, CategoryType Type, string Icon, string Color);
public sealed record CategoryResponse(Guid Id, string Name, CategoryType Type, string Icon, string Color, bool IsSystem)
{
    public static readonly Expression<Func<Category, CategoryResponse>> Projection = x => new(x.Id, x.Name, x.Type, x.Icon, x.Color, x.IsSystem);
}
public sealed class CategoryValidator : AbstractValidator<CategoryRequest>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Color).NotEmpty().Matches("^#[0-9a-fA-F]{6}$");
    }
}
