using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bams.Infrastructure;

public sealed class BranchScheduleConfiguration : IEntityTypeConfiguration<BranchSchedule>
{
    public void Configure(EntityTypeBuilder<BranchSchedule> e)
    {
        e.ToTable("branch_schedules");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.BranchId).HasColumnName("branch_id");
        e.Property(x => x.DayOfWeek).HasColumnName("day_of_week");
        e.Property(x => x.OpensAt).HasColumnName("opens_at");
        e.Property(x => x.ClosesAt).HasColumnName("closes_at");
        e.Property(x => x.IsActive).HasColumnName("is_active");
    }
}
