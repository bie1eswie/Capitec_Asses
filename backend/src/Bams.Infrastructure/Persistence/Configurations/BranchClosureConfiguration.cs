using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BranchClosureConfiguration : IEntityTypeConfiguration<BranchClosure>
{
    public void Configure(EntityTypeBuilder<BranchClosure> e)
    {
        e.ToTable("branch_closures");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.BranchId).HasColumnName("branch_id");
        e.Property(x => x.LocalDate).HasColumnName("local_date");
        e.Property(x => x.Reason).HasColumnName("reason");
    }
}
