using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BranchServiceConfiguration : IEntityTypeConfiguration<BranchService>
{
    public void Configure(EntityTypeBuilder<BranchService> e)
    {
        e.ToTable("branch_services");
        e.HasKey(x => new
        {
            x.BranchId,
            x.ServiceId
        });
        e.Property(x => x.BranchId).HasColumnName("branch_id");
        e.Property(x => x.ServiceId).HasColumnName("service_id");
        e.Property(x => x.Capacity).HasColumnName("capacity");
        e.Property(x => x.IsEnabled).HasColumnName("is_enabled");
        e.HasOne(x => x.Branch).WithMany(x => x.Services).HasForeignKey(x => x.BranchId);
        e.HasOne(x => x.Service).WithMany(x => x.Branches).HasForeignKey(x => x.ServiceId);
    }
}
