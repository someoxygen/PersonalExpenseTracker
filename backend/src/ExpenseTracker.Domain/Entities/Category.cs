using ExpenseTracker.Domain.Common;
namespace ExpenseTracker.Domain.Entities;

public enum CategoryType { Income, Expense }
public sealed class Category : Entity
{
    private Category() { }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = "";
    public string NormalizedName { get; private set; } = "";
    public CategoryType Type { get; private set; }
    public string Icon { get; private set; } = "";
    public string Color { get; private set; } = "";
    public bool IsSystem { get; private set; }
    public static Category Create(Guid userId, string name, CategoryType type, string icon, string color, bool isSystem = false)
    {
        if (!Enum.IsDefined(type)) throw new DomainException("Invalid category type.");
        var category = new Category { UserId = userId, Type = type, IsSystem = isSystem };
        category.SetDetails(name, icon, color);
        return category;
    }
    public void Update(string name, CategoryType type, string icon, string color)
    {
        EnsureEditable();
        if (type != Type) throw new DomainException("Category type cannot be changed. Create a new category.");
        SetDetails(name, icon, color);
        Touch();
    }
    public void EnsureEditable()
    {
        if (IsSystem) throw new DomainException("System categories cannot be changed or deleted.");
    }
    private void SetDetails(string name, string icon, string color)
    {
        Name = Rules.Text(name, 80);
        NormalizedName = Name.ToUpperInvariant();
        Icon = Rules.Text(icon, 40);
        Color = Rules.Text(color, 7);
    }
    public static IEnumerable<Category> Defaults(Guid userId)
    {
        foreach (var name in new[] { "Market", "Yemek", "Kira", "Fatura", "Ulaşım", "Eğlence", "Sağlık", "Eğitim", "Alışveriş", "Diğer" })
            yield return Create(userId, name, CategoryType.Expense, "receipt", "#D16B52", true);
        foreach (var name in new[] { "Maaş", "Freelance", "Yatırım", "Prim", "Diğer" })
            yield return Create(userId, name, CategoryType.Income, "wallet", "#278466", true);
    }
}
