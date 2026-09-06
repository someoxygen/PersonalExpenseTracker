using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.HasAlternateKey(x => new { x.UserId, x.Id });
        b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Name).HasMaxLength(80).IsRequired();
        b.Property(x => x.NormalizedName).HasMaxLength(80).IsRequired();
        b.Property(x => x.Icon).HasMaxLength(40).IsRequired();
        b.Property(x => x.Color).HasMaxLength(7).IsRequired();
        b.HasIndex(x => new { x.UserId, x.Type, x.NormalizedName }).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_Categories_Type", "\"Type\" IN (0,1)"));
    }
}
