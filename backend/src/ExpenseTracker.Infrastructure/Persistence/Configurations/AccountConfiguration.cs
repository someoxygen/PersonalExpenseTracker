using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.HasKey(x => x.Id);
        b.HasAlternateKey(x => new { x.UserId, x.Id });
        b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.InitialBalance).HasPrecision(18, 2);
        b.ToTable(t => t.HasCheckConstraint("CK_Accounts_Type", "\"Type\" BETWEEN 0 AND 4"));
    }
}
