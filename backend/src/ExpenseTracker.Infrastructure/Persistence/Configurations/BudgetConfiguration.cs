using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Category).WithMany().HasForeignKey(x => new { x.UserId, x.CategoryId }).HasPrincipalKey(x => new { x.UserId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.UserId, x.CategoryId, x.Month, x.Year }).IsUnique();
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Budgets_Amount", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_Budgets_Period", "\"Month\" BETWEEN 1 AND 12 AND \"Year\" BETWEEN 2000 AND 2100");
        });
    }
}
