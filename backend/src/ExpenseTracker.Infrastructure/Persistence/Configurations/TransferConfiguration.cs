using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.SourceAccount).WithMany().HasForeignKey(x => new { x.UserId, x.SourceAccountId }).HasPrincipalKey(x => new { x.UserId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TargetAccount).WithMany().HasForeignKey(x => new { x.UserId, x.TargetAccountId }).HasPrincipalKey(x => new { x.UserId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => new { x.UserId, x.TransactionDate });
        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Transfers_Amount", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_Transfers_Accounts", "\"SourceAccountId\" <> \"TargetAccountId\"");
        });
    }
}
