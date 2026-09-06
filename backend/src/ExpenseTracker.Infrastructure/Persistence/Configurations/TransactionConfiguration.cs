using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Account).WithMany().HasForeignKey(x => new { x.UserId, x.AccountId }).HasPrincipalKey(x => new { x.UserId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Category).WithMany().HasForeignKey(x => new { x.UserId, x.CategoryId }).HasPrincipalKey(x => new { x.UserId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => new { x.UserId, x.TransactionDate });
        b.HasIndex(x => new { x.UserId, x.AccountId });
        b.HasIndex(x => new { x.UserId, x.CategoryId });
        b.ToTable(t => { t.HasCheckConstraint("CK_Transactions_Amount", "\"Amount\" > 0"); t.HasCheckConstraint("CK_Transactions_Type", "\"Type\" IN (0,1)"); });
    }
}
