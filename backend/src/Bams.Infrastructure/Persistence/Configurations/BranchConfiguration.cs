using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> e)
    {
        e.ToTable("branches");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.Code).HasColumnName("code").HasMaxLength(24);
        e.Property(x => x.Name).HasColumnName("name").HasMaxLength(160);
        e.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
        e.Property(x => x.Address).HasColumnName("address").HasMaxLength(240);
        e.Property(x => x.TimeZone).HasColumnName("time_zone").HasMaxLength(100);
        e.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
        e.HasIndex(x => x.Code).IsUnique();
    }
}
